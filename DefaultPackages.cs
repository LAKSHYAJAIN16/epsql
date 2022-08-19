namespace Epsql.Packages.Default
{
    internal class DefaultPackages
    {
        public static string[] Find(string name)
        {
           if(name == "alpha")
           {
                return new string[]
                {
                    "^ w = WHERE",
                    "^ s = SELECT",
                    "^ p = PLACE",
                    "^ c = CREATE",
                    "^ t = TABLE"
                };
           }

           return new string[] { };
        }
    }
}
