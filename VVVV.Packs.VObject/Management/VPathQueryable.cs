using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VVVV.Packs.VObjects
{
    public abstract class VPathQueryable
    {
        public virtual object VPathGetItem(string key)
        {
            return null;
        }
        public virtual string[] VPathQueryKeys()
        {
            return null;
        }
        public void VPath(string path, List<object> Results, string Separator)
        {
            string[] levels = path.Split(Separator.ToCharArray());
            string nextpath = string.Join(Separator, levels, 1, levels.Length - 1);
            object tempObject;
            if ((levels[0][0] == '"') && (levels[0][levels[0].Length - 1] == '"'))
            {
                string key = levels[0].Trim('"');
                tempObject = VPathGetItem(key);
                if (tempObject != null)
                {
                    if (levels.Length == 1)
                    {
                        Results.Add(tempObject);
                        return;
                    }
                    VPathNextStep(key, nextpath, Results, Separator);
                }
            }
            else
            {
                Regex Pattern = new Regex(levels[0]);
                foreach (string k in VPathQueryKeys())
                {
                    if (Pattern.Match(k).Value != string.Empty)
                    {
                        if (levels.Length == 1)
                        {
                            Results.Add(VPathGetItem(k));
                        }
                        else VPathNextStep(k, nextpath, Results, Separator);
                    }
                }
                return;
            }
        }
        private void VPathNextStep(string CurrentLevel, string NextPath, List<object> Results, string Separator)
        {
            object tempObject = VPathGetItem(CurrentLevel);
            if (tempObject is VPathQueryable)
            {
                VPathQueryable vpq = tempObject as VPathQueryable;
                vpq.VPath(NextPath, Results, Separator);
            }
        }
        public List<object> VPath(string path, string Separator)
        {
            List<object> Results = new List<object>();
            this.VPath(path, Results, Separator);
            return Results;
        }
    }
}
