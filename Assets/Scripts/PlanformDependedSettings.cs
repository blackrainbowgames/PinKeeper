using OnePF;

namespace Assets.Scripts
{
    #if UNITY_ANDROID

    public static class PlanformDependedSettings
    {
        public static string StoreName = OpenIAB_Android.STORE_GOOGLE;
        public const string StorePublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAlq7Fz0J/hbVdmV52ZTbLSnPRh6UB6vT/XmvCPqH6rwXbqbSKL2E1cqqcPfiOFB3Uei5rEGrRxcpGPPwV6lOxnnb3lVqIgMigu4jcer+6duhcbN3ZRyrmaFcBEo8MezkdClyNTTrBfTnkvmc1T0bg3WjdgOAP+/MQq0W14CcaWPUVPSlBdQlhhcWbAWcRv37S35N/LbXEZV/BwC+1EwLcJISxkF9SHnkec89egoY0acSLLZmvYZm4rxU/UbrbJaaFEF+BFYlufxhPTQtN5xrAHAJGcC69SUaJL/B2/nznQ1MSD+vgkNo1Gb+HrT5MRwsIyqwRSVOUjj8tmMKfZGN9GQIDAQAB";
    }

    #endif

    #if UNITY_IPHONE

    public static class PlanformDependedSettings
    {
        public static string StoreName = OpenIAB_iOS.STORE;
        public const string StorePublicKey = "41f95fc5fdbe455a963a21b411f9024f";
    }

    #endif
}