using OnePF;

namespace Assets.Scripts
{
    #if UNITY_ANDROID

    public static class PlanformDependedSettings
    {
        public static string StoreName = OpenIAB_Android.STORE_GOOGLE;
        public const string StorePublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAgg6Z1KICLBmQnTd5CV/ivXjp8Jxg4FOeuFAu2tgQBxzcSnC0UMXug7VoB/9CU6RNznKQuMYKw+UoDl1la22SRg0Q7KxPFQ+rbmESM5eYSTh7O80YKeW4wgJKE4ZbGrnJbIZXgfw4c/ufZQY8/n+WrzXDgYYj0u2/PZsObGvGXjRwag7D7lhmmkQ89md6ukT8ChCgxbeFV5X3IPPhCxmdM/ptjRMUNCNnK4hhFCFKh16JioSry7OTJmeCYTZtvpVojSlzSxxB6jUZJMn5rfTqAaKtyVBX14Qgg+QyWYHaIelgp9wSJrxFaPn97bQsgK6Ophpz4Eh/JvpGfm1igZ34SwIDAQAB";
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