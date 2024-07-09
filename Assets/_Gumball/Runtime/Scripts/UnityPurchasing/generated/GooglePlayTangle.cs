// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("gLmIxK7z/YsvcqUXEukooDUyVsXkVtX25NnS3f5SnFIj2dXV1dHU185vOA8Ti9bNaq+Y9g+5PkuTjDLl3iDbc0OJMfJIB7UCXMcQcGJJaF3IrdrB3zGOp3YLJILM3sx+b7NMil5Yv9JhAUrm0rR6hwEvPHwaPvVwfQA4QBe0SAE6qug9CyxPmdkYLOdW1dvU5FbV3tZW1dXUHhhyrHIWRRaDcHhotfVeHlbJYehKdgNzkLH7wzfkYw8VQtTwhkp/9395bLNXqpuoj78d8XO75T5gjl563xyHYR8l6pcDVj2NGTHM6oUeIFJi3UoQvUvYen4cU/3gfATCLE53ChRIc9HR15CBQS8yr7rnsyZNpYnaFGnsb+HIDaNhUel3qPGUzdbX1dTV");
        private static int[] order = new int[] { 4,4,13,5,12,13,8,12,13,10,10,13,13,13,14 };
        private static int key = 212;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
