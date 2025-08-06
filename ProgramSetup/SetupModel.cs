using static PCL.Core.ProgramSetup.Helper;
using static PCL.Core.ProgramSetup.SetupEntrySource;

namespace PCL.Core.ProgramSetup;

public static class SetupModel
{
    public const int VersionCode = 1;

    public static class Identify
    {
        public static readonly SetupEntry Identifier = Global("Identify", "");
    }

    public static class Hint
    {
        public static readonly SetupEntry DownloadThreadCount = Global("HintDownloadThread", false);
        public static readonly SetupEntry Notice = Global("HintNotice", 0);
        public static readonly SetupEntry Download = Global("HintDownload", 0);
        public static readonly SetupEntry InstallPageBack = Global("HintInstallBack", false);
        public static readonly SetupEntry HideGameInstance = Global("HintHide", false);
        public static readonly SetupEntry ManualInstall = Global("HintHandInstall", false);
        public static readonly SetupEntry BuyGame = Global("HintBuy", false);
        public static readonly SetupEntry CleanJunkFile = Global("HintClearRubbish", 0);
        public static readonly SetupEntry UpdateMod = Global("HintUpdateMod", false);
        public static readonly SetupEntry MainpageCommand = Global("HintCustomCommand", false);
        public static readonly SetupEntry UntrustedMainpage = Global("HintCustomWarn", false);
        public static readonly SetupEntry MoreInstanceSetup = Global("HintMoreAdvancedSetup", false);
        public static readonly SetupEntry IndieSetup = Global("HintIndieSetup", false);
        public static readonly SetupEntry LaunchWithProfile = Global("HintProfileSelect", false);
        public static readonly SetupEntry ExportConfig = Global("HintExportConfig", false);
        public static readonly SetupEntry MaxGameLog = Global("HintMaxLog", false);
        public static readonly SetupEntry NonAsciiGamePath = Global("HintDisableGamePathCheckTip", false);
        public static readonly SetupEntry CommunityEdition = Global("UiLauncherCEHint", true);
        public static readonly SetupEntry CommunityEditionCount = Global("UiLauncherCEHintCount", 0);
    }

    public static class System
    {
        public static readonly SetupEntry Eula = Global("SystemEula", false);
        public static readonly SetupEntry StartupCount = Encrypted("SystemCount", 0);
        public static readonly SetupEntry LaunchCount = Encrypted("SystemLaunchCount", 0);
        public static readonly SetupEntry LastVersion = Encrypted("SystemLastVersionReg", 0);
        public static readonly SetupEntry LastSavedBetaVersion = Encrypted("SystemHighestSavedBetaVersionReg", 0);
        public static readonly SetupEntry LastBetaVersion = Encrypted("SystemHighestBetaVersionReg", 0);
        public static readonly SetupEntry LastAlphaVersion = Encrypted("SystemHighestAlphaVersionReg", 0);
        public static readonly SetupEntry SetupVersionGlobal = Global("SystemSetupVersionReg", VersionCode);
        public static readonly SetupEntry SetupVersionLocal = Local("SystemSetupVersionIni", VersionCode);
        public static readonly SetupEntry CacheDirectory = Global("SystemSystemCache", "");
        public static readonly SetupEntry UpdateSolution = Local("SystemSystemUpdate", 0);
        public static readonly SetupEntry UpdateBranch = Local("SystemSystemUpdateBranch", 0); // TODO
        public static readonly SetupEntry AnnounceSolution = Local("SystemSystemActivity", 0);
        public static readonly SetupEntry ShowedAnnouncements = Local("SystemSystemAnnouncement", "");
        public static readonly SetupEntry HttpProxy = Encrypted("SystemHttpProxy", "");
        public static readonly SetupEntry UseDefaultProxy = Global("SystemUseDefaultProxy", true);
        public static readonly SetupEntry NoHardwareAcceleration = Global("SystemDisableHardwareAcceleration", false);
        public static readonly SetupEntry Telemetry = Global("SystemTelemetry", false);
        public static readonly SetupEntry MirrorChyanKey = Encrypted("SystemMirrorChyanKey", "");
        public static readonly SetupEntry MaxGameLog = Global("SystemMaxLog", 13);
        public static readonly SetupEntry RandomUuid = Global("LaunchUuid", "");
        public static readonly SetupEntry LoginLegacyName = Encrypted("LoginLegacyName", "");
        public static readonly SetupEntry LoginMsJson = Encrypted("LoginMsJson", "{}");
        public static readonly SetupEntry LoginMsAuthType = Global("LoginMsAuthType", 1);

        public static class Debug
        {
            public static readonly SetupEntry Enabled = Global("SystemDebugMode", false);
            public static readonly SetupEntry AnimationSpeed = Global("SystemDebugAnim", 9);
            public static readonly SetupEntry AddRandomDelay = Global("SystemDebugDelay", false);
            public static readonly SetupEntry DontCopy = Global("SystemDebugSkipCopy", false);
        }
    }

    public static class Cache
    {
        public static readonly SetupEntry ExportConfigPath = Global("CacheExportConfig", "");
        public static readonly SetupEntry SavedMainpageUrl = Global("CacheSavedPageUrl", "");
        public static readonly SetupEntry SavedMainpageVersion = Global("CacheSavedPageVersion", "");
        public static readonly SetupEntry FileDownloadFolder = Global("CacheDownloadFolder", "");
        public static readonly SetupEntry DownloadUserAgent = Global("ToolDownloadCustomUserAgent", "");
        public static readonly SetupEntry JavaListVersion = Global("CacheJavaListVersion", 0);
        public static readonly SetupEntry AuthUuid = Encrypted("CacheAuthUuid", "");
        public static readonly SetupEntry AuthUserName = Encrypted("CacheAuthName", "");
        public static readonly SetupEntry AuthThirdPartyUserName = Encrypted("CacheAuthUsername", "");
        public static readonly SetupEntry AuthPassword = Encrypted("CacheAuthPass", "");
        public static readonly SetupEntry AuthServerAddress = Encrypted("CacheAuthServerServer", "");
    }

    public static class Link
    {
        public static readonly SetupEntry EulaAgreed = Global("LinkEula", false);
        public static readonly SetupEntry AnnounceCache = Encrypted("LinkAnnounceCache", "");
        public static readonly SetupEntry AnnounceCacheVer = Global("LinkAnnounceCache", 0);
        public static readonly SetupEntry RelayType = Global("LinkRelayType", 0);
        public static readonly SetupEntry ServerType = Global("LinkServerType", 1);
        public static readonly SetupEntry ProxyType = Global("LinkProxyType", 1);
        public static readonly SetupEntry RelayServer = Global("LinkRelayServer", "");
        public static readonly SetupEntry NaidRefreshToken = Encrypted("LinkNaidRefreshToken", "");
        public static readonly SetupEntry NaidRefreshExpireTime = Encrypted("LinkNaidRefreshExpiresAt", "");
        public static readonly SetupEntry DoFirstTimeNetTest = Encrypted("LinkFirstTimeNetTest", true);
    }

    public static class Tool
    {
        public static readonly SetupEntry CompFavorites = Global("CompFavorites", "[]");
        public static readonly SetupEntry FixAuthLib = Global("ToolFixAuthlib", true);
        public static readonly SetupEntry AutoChangeLanguage = Global("ToolHelpChinese", true);

        public static class Download
        {
            public static readonly SetupEntry ThreadCount = Global("ToolDownloadThread", 63);
            public static readonly SetupEntry SpeedLimit = Global("ToolDownloadSpeed", 42);
            public static readonly SetupEntry FileSourceSolution = Global("ToolDownloadSource", 1);
            public static readonly SetupEntry VersionSourceSolution = Global("ToolDownloadVersion", 1);
            public static readonly SetupEntry CompNameFormatV1 = Global("ToolDownloadTranslate", 0);
            public static readonly SetupEntry CompNameFormatV2 = Global("ToolDownloadTranslateV2", 1);
            public static readonly SetupEntry UiIgnoreQuilt = Global("ToolDownloadIgnoreQuilt", true);
            public static readonly SetupEntry ListenClipboard = Global("ToolDownloadClipboard", false);
            public static readonly SetupEntry EnableCertification = Global("ToolDownloadCert", true);
            public static readonly SetupEntry CompSourceSolution = Global("ToolDownloadMod", 1);
            public static readonly SetupEntry UiCompNameSolution = Global("ToolModLocalNameStyle", 0);
            public static readonly SetupEntry AutoSelectInstance = Global("ToolDownloadAutoSelectVersion", true);
        }

        public static class Update
        {
            public static readonly SetupEntry Alpha = Encrypted("ToolUpdateAlpha", 0);
            public static readonly SetupEntry Release = Global("ToolUpdateRelease", false);
            public static readonly SetupEntry Snapshot = Global("ToolUpdateSnapshot", false);
            public static readonly SetupEntry LastRelease = Global("ToolUpdateReleaseLast", "");
            public static readonly SetupEntry LastSnapshot = Global("ToolUpdateSnapshotLast", "");
        }
    }

    public static class Ui
    {
        public static readonly SetupEntry WindowHeight = Local("WindowHeight", 550);
        public static readonly SetupEntry WindowWidth = Local("WindowWidth", 900);
        public static readonly SetupEntry WindowOpacity = Local("UiLauncherTransparent", 600);
        public static readonly SetupEntry WindowHue = Local("UiLauncherHue", 180);
        public static readonly SetupEntry WindowSat = Local("UiLauncherSat", 80);
        public static readonly SetupEntry WindowDelta = Local("UiLauncherDelta", 90);
        public static readonly SetupEntry WindowLight = Local("UiLauncherLight", 20);
        public static readonly SetupEntry ThemeSelected = Local("UiLauncherTheme", 0);
        public static readonly SetupEntry ThemeGoldCode = Encrypted("UiLauncherThemeGold", "");
        public static readonly SetupEntry ThemeHiddenV1 = Encrypted("UiLauncherThemeGold", "0|1|2|3|4");
        public static readonly SetupEntry ThemeHiddenV2 = Encrypted("UiLauncherThemeGold", "0|1|2|3|4");
        public static readonly SetupEntry ShowStartupLogo = Local("UiLauncherLogo", true);
        public static readonly SetupEntry AdvanceBlur = Local("UiBlur", false);
        public static readonly SetupEntry AdvanceBlurValue = Local("UiBlurValue", 16);
        public static readonly SetupEntry BackgroundColorful = Local("UiBackgroundColorful", true);
        public static readonly SetupEntry WallpaperOpacity = Local("UiBackgroundOpacity", 1000);
        public static readonly SetupEntry WallpaperBlurRadius = Local("UiBackgroundBlur", 0);
        public static readonly SetupEntry WallpaperSuitMode = Local("UiBackgroundSuit", 0);
        public static readonly SetupEntry MainpageType = Local("UiCustomType", 0);
        public static readonly SetupEntry MainpageSelectedPreset = Local("UiCustomPreset", 0);
        public static readonly SetupEntry MainpageCustomUrl = Local("UiCustomNet", "");
        public static readonly SetupEntry DarkModeSolution = Global("UiDarkMode", 2);
        public static readonly SetupEntry DarkColor = Global("UiDarkColor", 1);
        public static readonly SetupEntry LightColor = Global("UiLightColor", 1);
        public static readonly SetupEntry LockWindowSize = Global("UiLockWindowSize", false);
        public static readonly SetupEntry LogoSolution = Local("UiLogoType", 1);
        public static readonly SetupEntry LogoCustomText = Local("UiLogoText", "");
        public static readonly SetupEntry TopBarLeftAlign = Local("UiLogoLeft", false);
        public static readonly SetupEntry AnimationFpsLimit = Global("UiAniFPS", 59);
        public static readonly SetupEntry Font = Local("UiFont", "");

        public static class Music
        {
            public static readonly SetupEntry StopInGame = Local("UiMusicStop", false);
            public static readonly SetupEntry StartInGame = Local("UiMusicStart", false);
            public static readonly SetupEntry ShufflePlayback = Local("UiMusicRandom", true);
            public static readonly SetupEntry SmtcEnabled = Local("UiMusicSMTC", true);
            public static readonly SetupEntry StartOnStartup = Local("UiMusicAuto", true);
        }

        public static class Hide
        {
            public static readonly SetupEntry PageDownload = Local("UiHiddenPageDownload", false);
            public static readonly SetupEntry PageLink = Local("UiHiddenPageLink", false);
            public static readonly SetupEntry PageSetup = Local("UiHiddenPageSetup", false);
            public static readonly SetupEntry PageOther = Local("UiHiddenPageOther", false);
            public static readonly SetupEntry FunctionSelect = Local("UiHiddenFunctionSelect", false);
            public static readonly SetupEntry FunctionModUpdate = Local("UiHiddenFunctionModUpdate", false);
            public static readonly SetupEntry FunctionHidden = Local("UiHiddenFunctionHidden", false);
            public static readonly SetupEntry SetupLaunch = Local("UiHiddenSetupLaunch", false);
            public static readonly SetupEntry SetupUi = Local("UiHiddenSetupUi", false);
            public static readonly SetupEntry SetupSystem = Local("UiHiddenSetupSystem", false);
            public static readonly SetupEntry OtherHelp = Local("UiHiddenOtherHelp", false);
            public static readonly SetupEntry OtherFeedback = Local("UiHiddenOtherFeedback", false);
            public static readonly SetupEntry OtherVote = Local("UiHiddenOtherVote", false);
            public static readonly SetupEntry OtherAbout = Local("UiHiddenOtherAbout", false);
            public static readonly SetupEntry OtherTest = Local("UiHiddenOtherTest", false);
            public static readonly SetupEntry InstanceEdit = Local("UiHiddenVersionEdit", false);
            public static readonly SetupEntry InstanceExport = Local("UiHiddenVersionExport", false);
            public static readonly SetupEntry InstanceSave = Local("UiHiddenVersionSave", false);
            public static readonly SetupEntry InstanceScreenshot = Local("UiHiddenVersionScreenshot", false);
            public static readonly SetupEntry InstanceMod = Local("UiHiddenVersionMod", false);
            public static readonly SetupEntry InstanceResourcePack = Local("UiHiddenVersionResourcePack", false);
            public static readonly SetupEntry InstanceShader = Local("UiHiddenVersionShader", false);
            public static readonly SetupEntry InstanceSchematic = Local("UiHiddenVersionSchematic", false);
        }
    }

    public static class Launch
    {
        private const string JvmArgsDefault =
            "-XX:+UseG1GC -XX:-UseAdaptiveSizePolicy -XX:-OmitStackTraceInFastThrow " +
            "-Djdk.lang.Process.allowAmbiguousCommands=true -Dfml.ignoreInvalidMinecraftCertificates=True " +
            "-Dfml.ignorePatchDiscrepancies=True -Dlog4j2.formatMsgNoLookups=true";

        public static readonly SetupEntry SelectedInstance = Local("LaunchInstanceSelect", "");
        public static readonly SetupEntry SelectedFolder = Local("LaunchFolderSelect", "");
        public static readonly SetupEntry Folders = Global("LaunchFolderSelect", "");
        public static readonly SetupEntry MemorySolution = Local("LaunchRamType", 0);
        public static readonly SetupEntry CustomMemorySize = Local("LaunchRamCustom", 15);
        public static readonly SetupEntry PreferredIpStack = Global("LaunchPreferredIpStack", 1);
        public static readonly SetupEntry OptimizeMemory = Global("LaunchArgumentRam", false);
        public static readonly SetupEntry JvmArgs = Local("LaunchAdvanceJvm", JvmArgsDefault);
        public static readonly SetupEntry GameArgs = Local("LaunchAdvanceGame", "");
        public static readonly SetupEntry PreLaunchCommand = Local("LaunchAdvanceRun", "");
        public static readonly SetupEntry PreLaunchCommandWait = Local("LaunchAdvanceRunWait", "");
        public static readonly SetupEntry DisableJlw = Local("LaunchAdvanceDisableJLW", false);
        public static readonly SetupEntry DisableRw = Local("LaunchAdvanceDisableRW", false);
        public static readonly SetupEntry SetGpuPreference = Global("LaunchAdvanceGraphicCard", true);
        public static readonly SetupEntry DontUseJavaw = Global("LaunchAdvanceNoJavaw", false);
        public static readonly SetupEntry Title = Local("LaunchArgumentTitle", "");
        public static readonly SetupEntry TypeInfo = Local("LaunchArgumentInfo", "PCL");
        public static readonly SetupEntry SelectedJava = Global("LaunchArgumentJavaSelect", "");
        public static readonly SetupEntry Javas = Global("LaunchArgumentJavaUser", "[]");
        public static readonly SetupEntry IndieSolutionV1 = Local("LaunchArgumentIndie", 0);
        public static readonly SetupEntry IndieSolutionV2 = Local("LaunchArgumentIndieV2", 4);
        public static readonly SetupEntry LauncherVisibility = Global("LaunchArgumentVisible", 5);
        public static readonly SetupEntry ProcessPriority = Global("LaunchArgumentPriority", 1);
        public static readonly SetupEntry WindowWidth = Local("LaunchArgumentWindowWidth", 854);
        public static readonly SetupEntry WindowHeight = Local("LaunchArgumentWindowHeight", 480);
        public static readonly SetupEntry WindowType = Local("LaunchArgumentWindowType", 1);
    }

    public static class Instance
    {
        public static readonly SetupEntry JvmArgs = Instance("VersionAdvanceJvm", "");
        public static readonly SetupEntry GameArgs = Instance("VersionAdvanceGame", "");
        public static readonly SetupEntry AssetVerifySolutionV1 = Instance("VersionAdvanceAssets", 0);
        public static readonly SetupEntry DisableAssetVerifyV2 = Instance("VersionAdvanceAssetsV2", false);
        public static readonly SetupEntry IgnoreIncompatibleJava = Instance("VersionAdvanceJava", false);
        public static readonly SetupEntry DisableJlwObsolete = Instance("VersionAdvanceDisableJlw", false);
        public static readonly SetupEntry PreLaunchCommand = Instance("VersionAdvanceRun", "");
        public static readonly SetupEntry PreLaunchCommandWait = Instance("VersionAdvanceRunWait", true);
        public static readonly SetupEntry DisableJlw = Instance("VersionAdvanceDisableJLW", false);
        public static readonly SetupEntry UseProxy = Instance("VersionAdvanceUseProxyV2", false);
        public static readonly SetupEntry DisableRw = Instance("VersionAdvanceDisableRW", false);
        public static readonly SetupEntry MemorySolution = Instance("VersionRamType", 2);
        public static readonly SetupEntry CustomMemorySize = Instance("VersionRamCustom", 15);
        public static readonly SetupEntry OptimizeMemory = Instance("VersionRamOptimize", 0);
        public static readonly SetupEntry Title = Instance("VersionArgumentTitle", "");
        public static readonly SetupEntry UseGlobalTitle = Instance("VersionArgumentTitleEmpty", false);
        public static readonly SetupEntry TypeInfo = Instance("VersionArgumentInfo", "");
        public static readonly SetupEntry IndieV1 = Instance("VersionArgumentIndie", -1);
        public static readonly SetupEntry IndieV2 = Instance("VersionArgumentIndieV2", false);
        public static readonly SetupEntry SelectedJava = Instance("VersionArgumentJavaSelect", "使用全局设置");
        public static readonly SetupEntry ServerToEnter = Instance("VersionServerEnter", "");
        public static readonly SetupEntry LoginRequirementSolution = Instance("VersionServerLoginRequire", 0);
        public static readonly SetupEntry AuthRegisterAddress = Instance("VersionServerAuthRegister", "");
        public static readonly SetupEntry AuthServerDisplayName = Instance("VersionServerAuthName", "");
        public static readonly SetupEntry AuthServerAddress = Instance("VersionServerAuthServer", "");
        public static readonly SetupEntry AuthTypeLucked = Instance("VersionServerLoginLock", false);
        public static readonly SetupEntry LaunchCount = Instance("VersionLaunchCount", 0);
    }
}

file static class Helper
{
    public static SetupEntry Encrypted(string keyName, object defaultValue) =>
        new(SystemGlobal, keyName, defaultValue, true);

    public static SetupEntry Global(string keyName, object defaultValue) => new(SystemGlobal, keyName, defaultValue);
    public static SetupEntry Local(string keyName, object defaultValue) => new(PathLocal, keyName, defaultValue);
    public static SetupEntry Instance(string keyName, object defaultValue) => new(GameInstance, keyName, defaultValue);
}