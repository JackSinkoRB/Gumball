// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("jqmZO9dVncMYRqh4XPk6oUc5A8zoSR4pNa3w60yJvtApnxhttaoUwzClVl5Ok9N4OHDvR85sUCVVtpfd+Ab9VWWvF9RuIZMkeuE2VkRvTnt4fpn0RydswPSSXKEnCRpaPBjTVsJw89DC//T72HS6dAX/8/Pz9/LxXFg6ddvGWiLkCmhRLDJuVff38bamn67iiNXbrQlUgzE0zw6GExRw4+6L/Of5F6iBUC0CpOr46lhJlWqsp2cJFImcwZUAa4Ov/DJPyknH7itbJh5mMZJuJxyMzhstCmm//z4KweURwkUpM2Ty1qBsWdFZX0qVcYy9cPP98sJw8/jwcPPz8jg+VIpUMGOxJXAbqz8X6syjOAZ0RPtsNptt/oVHd89Rjtey6/Dx8/Lz");
        private static int[] order = new int[] { 9,13,3,5,5,9,11,9,12,9,12,12,13,13,14 };
        private static int key = 242;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
