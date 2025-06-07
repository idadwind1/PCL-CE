using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using PCL.Core.Utils.PE;
using PCL.Core.Helper;

namespace PCL.Core.Model
{
    public enum JavaBrandType
    {
        EclipseTemurin,
        Microsoft,
        Bellsoft,
        AzulZulu,
        AmazonCorretto,
        IBMSemeru,
        Oracle,
        Dragonwell,
        TencentKona,
        OpenJDK,
        Unknown
    }
    public class Java
    {
        /// <summary>
        /// 就像这样：
        /// D:\Program Files\Java24\bin
        /// </summary>
        public string JavaFolder { get; set; }
        public Version Version { get; set; }
        public int JavaMajorVersion => Version.Major == 1
            ? Version.Minor
            : Version.Major;
        public JavaBrandType Brand { get; set; }
        /// <summary>
        /// 用户是否启动此 Java
        /// </summary>
        public bool IsEnabled { get; set; }
        public bool Is64Bit { get; set; }
        public bool IsJre { get; set; }
        public string JavaExePath => $@"{JavaFolder}\java.exe";
        public string JavawExePath => $@"{JavaFolder}\javaw.exe";

        public override string ToString()
        {
            return $" {(IsJre?"JRE":"JDK")} {JavaMajorVersion} {Brand} {(Is64Bit ? "64 Bit" : "32 Bit")} | {JavaFolder}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Java model)
            {
                return Path.Equals(model.JavaFolder, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return JavaFolder?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// 通过路径获取 Java 实例化信息，如果 Java 信息出现错误返回 null
        /// </summary>
        /// <param name="JavaExePath">java.exe 的文件地址</param>
        /// <returns></returns>
        public static Java Prase(string JavaExePath)
        {
            try
            {
                if (!File.Exists(JavaExePath))
                    return null;
                var JavaFileVersion = FileVersionInfo.GetVersionInfo(JavaExePath);
                var JavaVersion = Version.Parse(JavaFileVersion.FileVersion);
                var CompanyName = JavaFileVersion.CompanyName
                    ?? JavaFileVersion.FileDescription
                    ?? JavaFileVersion.ProductName
                    ?? string.Empty;
                if (CompanyName == "N/A") // 某 O 开头的 Java 信息不写全
                    CompanyName = JavaFileVersion.FileDescription;
                var JavaBrand = DetermineBrand(CompanyName);

                var CurrentJavaFolder = Path.GetDirectoryName(JavaExePath);
                var IsJavaJre = !File.Exists(Path.Combine(CurrentJavaFolder, "javac.exe"));
                var PEData = PEHeaderReader.ReadPEHeader(JavaExePath);
                var IsJava64Bit = PEHeaderReader.IsMachine64Bit(PEData.Machine);
                var ShouldDisableByDefault = (IsJavaJre && JavaVersion.Major >= 16)
                    || (!IsJava64Bit && Environment.Is64BitOperatingSystem);

                return new Java
                {
                    JavaFolder = CurrentJavaFolder,
                    Version = JavaVersion,
                    IsJre = IsJavaJre,
                    Is64Bit = IsJava64Bit,
                    IsEnabled = !ShouldDisableByDefault,
                    Brand = JavaBrand
                };
            }
            catch { /* 忽略无法获取版本的Java路径 */}
            return null;
        }

        private static JavaBrandType DetermineBrand(string output)
        {
            if (output.IndexOf("Eclipse", StringComparison.OrdinalIgnoreCase) >= 0
                || output.IndexOf("Temurin", StringComparison.OrdinalIgnoreCase) >= 0)
                return JavaBrandType.EclipseTemurin;
            if (output.IndexOf("Bellsoft", StringComparison.OrdinalIgnoreCase) >= 0)
                return JavaBrandType.Bellsoft;
            if (output.IndexOf("Microsoft", StringComparison.OrdinalIgnoreCase) >= 0)
                return JavaBrandType.Microsoft;
            if (output.IndexOf("Amazon", StringComparison.OrdinalIgnoreCase) >= 0)
                return JavaBrandType.AmazonCorretto;
            if (output.IndexOf("Azul", StringComparison.OrdinalIgnoreCase) >= 0)
                return JavaBrandType.AzulZulu;
            if (output.IndexOf("IBM", StringComparison.OrdinalIgnoreCase) >= 0)
                return JavaBrandType.IBMSemeru;
            if (output.IndexOf("Oracle", StringComparison.OrdinalIgnoreCase) >= 0)
                return JavaBrandType.OpenJDK;
            if (output.IndexOf("Tencent", StringComparison.OrdinalIgnoreCase) >= 0)
                return JavaBrandType.TencentKona;
            if (output.IndexOf("Java(TM)", StringComparison.OrdinalIgnoreCase) >= 0)
                return JavaBrandType.Oracle;
            if (output.IndexOf("Alibaba", StringComparison.OrdinalIgnoreCase) >= 0)
                return JavaBrandType.Dragonwell;
            return JavaBrandType.Unknown;
        }
    }
}