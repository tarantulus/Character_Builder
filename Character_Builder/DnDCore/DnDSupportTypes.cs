using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;

namespace DnDSupportTypes
{
    public static class DnDCore
    {
        public static bool hasLoadSuccess = false;
        public static bool verbose = false;
        public static List<Exception> BOOM = new List<Exception>();

        public static List<DnDRace> Races = new List<DnDRace>();
        public static List<DnDClass> Classes = new List<DnDClass>();
        public static List<DnDSubClass> SubClasses = new List<DnDSubClass>();
        public static List<DnDFeature> Features = new List<DnDFeature>();
        public static List<DnDSkill> Skills = new List<DnDSkill>();
        public static List<string> Attributes = new List<string>();
        public static List<int> StartingScores = new List<int>();

        public static List<string> Names_Male = new List<string>();
        public static List<string> Names_Female = new List<string>();
        public static List<string> Names_Last = new List<string>();

        static DnDCore()
        {
            try
            {
                initFeatures();
                initRaces();
                initClasses();
                initSubClasses();
                initNames();
                initSkills();

                Attributes = new List<string>() { "Str", "Dex", "Con", "Int", "Wis", "Cha" };
                StartingScores = new List<int>() { 15, 14, 13, 12, 10, 8 };

                // this has to be the last line!
                hasLoadSuccess = true;
            }
            catch (Exception ex)
            {
                BOOM.Add(ex);
                hasLoadSuccess = false;
            }
        }

        private static void initSubClasses()
        {
            using (StreamReader sr = new StreamReader("DnDCore\\SubClasses.json"))
            {
                var classJSON = JsonConvert.DeserializeObject<List<DnDSubClass>>(sr.ReadToEnd());

                foreach (var r in classJSON)
                {
                    SubClasses.Add(r);
                }
            }
        }

        private static void initClasses()
        {
            using (StreamReader sr = new StreamReader("DnDCore\\Classes.json"))
            {
                var classJSON = JsonConvert.DeserializeObject<List<DnDClass>>(sr.ReadToEnd());

                foreach (var r in classJSON)
                {
                    Classes.Add(r);
                }
            }
        }

        private static void initRaces()
        {
            using (StreamReader sr = new StreamReader("DnDCore\\Races.json"))
            {
                var raceJSON = JsonConvert.DeserializeObject<List<DnDRace>>(sr.ReadToEnd());
                List<DnDRace> tempBase = new List<DnDRace>();

                foreach (var r in raceJSON)
                {
                    // base races with subraces
                    if (r.BaseRace.Equals(""))
                    {
                        if (!tempBase.Contains(r))
                        {
                            tempBase.Add(r);
                        }
                    }
                    // base races with NO subraces
                    else if (r.BaseRace.Equals(r.Name))
                    {
                        Races.Add(r);
                    }
                    // subraces
                    else
                    {
                        var adding = tempBase.First(z => z.Name.Equals(r.BaseRace));
                        adding += r;

                        Races.Add(adding);
                    }
                }
            }
        }

        private static void initFeatures()
        {
            using (StreamReader sr = new StreamReader("DnDCore\\Features.json"))
            {
                var featureJSON = JsonConvert.DeserializeObject<List<DnDFeature>>(sr.ReadToEnd());

                foreach (var f in featureJSON)
                {
                    Features.Add(f);
                }
            }
        }

        private static void initNames()
        {
            using (StreamReader sr = new StreamReader("DnDCore\\Names.json"))
            {
                var NamesJSON = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(sr.ReadToEnd());

                foreach (var n in NamesJSON["Male"])
                {
                    Names_Male.Add(n);
                }
                foreach (var n in NamesJSON["Female"])
                {
                    Names_Female.Add(n);
                }
                foreach (var n in NamesJSON["Last"])
                {
                    Names_Last.Add(n);
                }
            }
        }

        private static void initSkills()
        {
            using (StreamReader sr = new StreamReader("DnDCore\\Skills.json"))
            {
                var skillJSON = JsonConvert.DeserializeObject<List<DnDSkill>>(sr.ReadToEnd());

                foreach (var s in skillJSON)
                {
                    Skills.Add(s);
                }
            }
        }
    }

    public class DnDRace
    {
        public string Name;
        public string BaseRace;
        public string Size;
        public int Speed;
        public List<string> Languages = new List<string>();
        public List<DnDFeature> RacialFeatures = new List<DnDFeature>();
        public Dictionary<string, int> AbilityChanges = new Dictionary<string, int>();

        public DnDRace(string name, string baserace, string size, int speed, List<string> languages, List<string> racialFeatures, Dictionary<string, int> abilityChanges)
        {
            Name = name;
            BaseRace = baserace;
            Size = size;
            Speed = speed;
            Languages = languages;
            foreach (string thing in racialFeatures)
            {
                RacialFeatures.Add(DnDCore.Features.Find(z => z.Name.Equals(thing)));
            }
            AbilityChanges = abilityChanges;
        }

        private DnDRace(string name, string baserace, string size, int speed, List<string> languages, List<DnDFeature> racialFeatures, Dictionary<string, int> abilityChanges)
        {
            Name = name;
            BaseRace = baserace;
            Size = size;
            Speed = speed;
            Languages = languages;
            RacialFeatures = racialFeatures;
            AbilityChanges = abilityChanges;
        }

        public static DnDRace operator +(DnDRace baseRace, DnDRace subRace)
        {
            string endname = subRace.Name;
            string endbase = subRace.BaseRace;
            string endsize = baseRace.Size;
            int endspeed = subRace.Speed > 0 ? subRace.Speed : baseRace.Speed;
            List<string> endlang = baseRace.Languages.Union(subRace.Languages).ToList();
            List<DnDFeature> endfeatures = baseRace.RacialFeatures.Union(subRace.RacialFeatures).ToList();

            Dictionary<string, int> endabilities = baseRace.AbilityChanges;
            foreach (var t in subRace.AbilityChanges)
            {
                if (!endabilities.ContainsKey(t.Key))
                {
                    endabilities.Add(t.Key, t.Value);
                }
                else
                {
                    endabilities[t.Key] += t.Value;
                }
            }

            return new DnDRace(endname, endbase, endsize, endspeed, endlang, endfeatures, endabilities);
        }

        public override string ToString()
        {
            string race = $"Race: {Name}\nSize: {Size}\nSpeed: {Speed}\nLanguages: ";
            foreach (var l in Languages)
            {
                race += $"{l}, ";
            }
            race = race.Remove(race.Length - 2, 1) + "\nRacial Features: ";
            foreach (var rf in RacialFeatures)
            {
                race += $"{rf.Name}, ";
            }
            race = race.Remove(race.Length - 2, 1) + "\nAbility Changes: ";
            foreach (var ac in AbilityChanges)
            {
                race += $"{ac.Key}" + (ac.Value >= 0 ? $"+{ac.Value}" : $"-{ac.Value}") + ", ";
            }
            race = race.Remove(race.Length - 2, 1) + "\n";

            return race;
        }
    }

    public class DnDClass
    {
        public string Name;
        public int HitDieType;
        public List<string> PrimaryAbilities = new List<string>();
        public List<string> SaveProficiencies = new List<string>();
        public List<string> GearProficiencies = new List<string>();
        public Dictionary<string, int> Progression = new Dictionary<string, int>();
        public int numSkills;
        public List<string> ClassSkills = new List<string>();

        public override string ToString()
        {
            string outclass = $"{Name}\nHit Die: {HitDieType}\nPrimary Abilities:";
            foreach (var pa in PrimaryAbilities)
            {
                outclass += $" {pa},";
            }
            outclass = outclass.Remove(outclass.Length - 1, 1) + "\nSave Proficiencies:";
            foreach (var sp in SaveProficiencies)
            {
                outclass += $" {sp},";
            }
            outclass = outclass.Remove(outclass.Length - 1, 1) + "\nGear Proficiencies:";
            foreach (var gp in GearProficiencies)
            {
                outclass += $" {gp},";
            }
            outclass = outclass.Remove(outclass.Length - 1, 1) + $"\nClass Skills: {numSkills} from the following:";
            foreach (var cs in ClassSkills)
            {
                outclass += $" {cs},";
            }
            outclass = outclass.Remove(outclass.Length - 1, 1) + $"\nFeature Progression:\n";

            var Prog = populateProgression();

            foreach (var cp in Prog)
            {
                outclass += $"{cp.Key}\t{cp.Value}\n";
            }
            return outclass;
        }

        private Dictionary<int, string> populateProgression()
        {
            var temp = new Dictionary<int, string>();
            foreach (var p in Progression)
            {
                if (!temp.ContainsKey(p.Value))
                {
                    temp.Add(p.Value, p.Key);
                }
                else
                {
                    temp[p.Value] += $", {p.Key}";
                }
            }
            return temp.OrderBy(z => z.Key).ToDictionary(k => k.Key, v => v.Value);
        }
    }

    public class DnDSubClass
    {
        public string Name;
        public string BaseClass;
        public Dictionary<string, int> Progression = new Dictionary<string, int>();

        public override string ToString()
        {
            string outclass = $"({BaseClass}) {Name}\nFeature Progression:\n";
            var Prog = populateProgression();
            foreach (var cp in Prog)
            {
                outclass += $"{cp.Key}\t{cp.Value}\n";
            }

            return outclass;
        }

        private Dictionary<int, string> populateProgression()
        {
            var temp = new Dictionary<int, string>();
            foreach (var p in Progression)
            {
                if (!temp.ContainsKey(p.Value))
                {
                    temp.Add(p.Value, p.Key);
                }
                else
                {
                    temp[p.Value] += $", {p.Key}";
                }
            }
            return temp.OrderBy(z => z.Key).ToDictionary(k => k.Key, v => v.Value);
        }
    }

    public class DnDFeature
    {
        public string Name;
        public string Description;

        public override string ToString()
        {
            return $"{Name} - {Description}";
        }
    }

    public class DnDSkill
    {
        public string Name;
        public string AbilityMod;

        public DnDSkill(string name, string ability)
        {
            Name = name;
            AbilityMod = ability;
        }

        public override string ToString()
        {
            return $"({AbilityMod}){Name}";
        }
    }

    public class DnDBackground
    {
        // todo: figure out this mess
    }

    public class DnDCharacter
    {
        private static Random rand = new Random();

        public string Name = "";

        public bool isMale = true;
        public DnDRace Race = null;
        public DnDBackground Background;
        public DnDClass Class = null;
        public DnDSubClass SubClass = null;
        public static string ProficiencyBonus = "1 + (Level / 4), rounded up";
        public int AC = 0;
        public int Initiative = 0;
        public int Speed = 0;
        public string GearProficiencies = "";
        public List<DnDFeature> ClassFeatures = new List<DnDFeature>();
        public List<DnDFeature> RacialFeatures = new List<DnDFeature>();
        public Dictionary<int, string> FeatureProgression = new Dictionary<int, string>();
        public List<DnDSkill> Skills = new List<DnDSkill>();

        public Dictionary<string, int> Attributes = new Dictionary<string, int>();
        public Dictionary<string, int> AttributeMods = new Dictionary<string, int>();
        public Dictionary<string, int> Saves = new Dictionary<string, int>();

        public DnDCharacter()
        {
            pickSex();
            createName();

            setRace();
            setClass();
            setSubClass();
            populateProgression();
            setAttributes();
            setSaves();
        }

        private void setSaves()
        {
            Saves = new Dictionary<string, int>();
        }

        private void setAttributes()
        {
            var tempAttr = DnDCore.StartingScores.ToList();
            foreach (var attr in DnDCore.Attributes)
            {
                int t = rand.Next(0, tempAttr.Count);
                Attributes[attr] = tempAttr[t];
                tempAttr.RemoveAt(t);
                //todo: pull attribute mods into a different function that is calculated AFTER the race mods are applied
                AttributeMods[attr] = AttrMod(Attributes[attr]);
            }
        }

        private void setSubClass()
        {
            var temp = DnDCore.SubClasses.Where(z => z.BaseClass.Equals(Class.Name)).ToList();
            SubClass = temp[rand.Next(0, temp.Count)];

            foreach (var f in SubClass.Progression)
            {
                try
                {
                    ClassFeatures.Add(DnDCore.Features.Single(z => z.Name.Equals(f.Key)));
                }
                catch (InvalidOperationException ioe)
                {
                    MessageBox.Show(ioe.Message);
                }
            }
        }

        private void setClass()
        {
            Class = DnDCore.Classes[rand.Next(DnDCore.Classes.Count)];

            foreach (var f in Class.Progression)
            {
                try
                {
                    ClassFeatures.Add(DnDCore.Features.Single(z => z.Name.Equals(f.Key)));
                }
                catch (InvalidOperationException ioe)
                {
                    MessageBox.Show(ioe.Message);
                }
            }
        }

        private void setRace()
        {
            Race = DnDCore.Races[rand.Next(DnDCore.Races.Count)];

            foreach (var r in Race.RacialFeatures)
            {
                RacialFeatures.Add(r);
            }
        }

        private void pickSex()
        {
            isMale = rand.NextDouble() < .5;
        }

        private int AttrMod(int attributeScore)
        {
            return (int)Math.Floor((double)(attributeScore - 10) / 2);
        }

        private void createName()
        {
            string name = "";
            if (isMale)
            {
                name = DnDCore.Names_Male[rand.Next(DnDCore.Names_Male.Count)];
            }
            else
            {
                name = DnDCore.Names_Female[rand.Next(DnDCore.Names_Female.Count)];
            }

            name += " " + DnDCore.Names_Last[rand.Next(DnDCore.Names_Last.Count)];
            Name = name;
        }

        public override string ToString()
        {
            bool verbose = DnDCore.verbose;
            string output = $"{Name}\n{Race.Name} {Class.Name} ({SubClass.Name})\nProficiency Bonus: +{ProficiencyBonus}\n\n";
            foreach (var kvp in Attributes)
            {
                output += $"{kvp.Key}  {kvp.Value},  mod {AttributeMods[kvp.Key]}\n";
            }

            output += $"\nRacial Features\n";
            foreach (var f in RacialFeatures)
            {
                output += verbose ? $"\n{f.ToString()}\n" : $"{f.Name}, ";
            }
            if (!verbose) { output = output.Remove(output.Length - 2, 2) + "\n"; }

            output += $"\nFeature Progression\n";
            foreach (var f in FeatureProgression)
            {
                string temp = "";
                if (verbose)
                {
                    temp = $"Level {f.Key}\n";
                    var stemp = f.Value.Replace(", ", ",");
                    foreach (var s in stemp.Split(','))
                    {
                        temp += $"\t{DnDCore.Features.First(z => z.Name.Equals(s)).ToString()}\n";
                    }
                }
                else
                {
                    temp = $"{f.Key}\t{f.Value}";
                }
                output += $"{temp}\n";
            }





            return output;
        }

        private void populateProgression()
        {
            var supertemp = Class.Progression.Union(SubClass.Progression).ToDictionary(k => k.Key, v => v.Value);
            var megatemp = new Dictionary<int, string>();
            foreach (var p in supertemp)
            {
                if (!megatemp.ContainsKey(p.Value))
                {
                    megatemp.Add(p.Value, p.Key);
                }
                else
                {
                    megatemp[p.Value] += $", {p.Key}";
                }
            }
            FeatureProgression = megatemp.OrderBy(z => z.Key).ToDictionary(k => k.Key, v => v.Value);
        }
    }
}