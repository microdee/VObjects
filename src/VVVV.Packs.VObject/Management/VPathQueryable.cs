using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VVVV.Packs.VObjects
{
    public interface IVPathQueryable
    {
        object VPathGetItem(string key);
        string[] VPathQueryKeys();
    }
    public static class VPathQueryableExtensions
    {
        public static void VPath(this IVPathQueryable vpq, string path, List<object> Results, string Separator)
        {
            string[] levels = path.Split(Separator.ToCharArray());
            string nextpath = string.Join(Separator, levels, 1, levels.Length - 1);
            if ((levels[0][0] == '"') && (levels[0][levels[0].Length - 1] == '"'))
            {
                string key = levels[0].Trim('"');
                var tempObject = vpq.VPathGetItem(key);
                if (tempObject != null)
                {
                    if (levels.Length == 1)
                    {
                        Results.Add(tempObject);
                        return;
                    }
                    vpq.VPathNextStep(key, nextpath, Results, Separator);
                }
            }
            else
            {
                Regex Pattern = new Regex(levels[0]);
                foreach (string k in vpq.VPathQueryKeys())
                {
                    if (Pattern.Match(k).Value != string.Empty)
                    {
                        if (levels.Length == 1)
                        {
                            Results.Add(vpq.VPathGetItem(k));
                        }
                        else vpq.VPathNextStep(k, nextpath, Results, Separator);
                    }
                }
            }
        }
        public static void VPathNextStep(this IVPathQueryable vpq, string CurrentLevel, string NextPath, List<object> Results, string Separator)
        {
            object tempObject = vpq.VPathGetItem(CurrentLevel);
            if (tempObject is IVPathQueryable)
            {
                IVPathQueryable tvpq = tempObject as IVPathQueryable;
                tvpq.VPath(NextPath, Results, Separator);
            }
        }
        public static List<object> VPath(this IVPathQueryable vpq, string path, string Separator)
        {
            List<object> Results = new List<object>();
            vpq.VPath(path, Results, Separator);
            return Results;
        }
    }
}
