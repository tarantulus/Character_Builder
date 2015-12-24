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
        public static bool loaded = false;
        public static bool verbose = false;
        public static List<DnDRace> Races = new List<DnDRace>();
        public static List<DnDClass> Classes = new List<DnDClass>();
        public static List<DnDFeature> Features = new List<DnDFeature>();
        public static List<DnDSkill> Skills = new List<DnDSkill>();
        public static List<string> Attributes = new List<string>();
        public static List<int> StartingScores = new List<int>();

        public static List<string> Names_Male = new List<string>();
        public static List<string> Names_Female = new List<string>();
        public static List<string> Names_Last = new List<string>();

        static DnDCore()
        {
            initFeatures();
            initRaces();
            initClasses();
            initNames();
            initSkills();

            Attributes = new List<string>() { "Str", "Dex", "Con", "Int", "Wis", "Cha" };
            StartingScores = new List<int>() { 15, 14, 13, 12, 10, 8 };


            // this has to be the last line!
            loaded = true;
        }

        private static void initClasses()
        {
            try
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
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private static void initRaces()
        {
            try
            {
                using (StreamReader sr = new StreamReader("DnDCore\\Races.json"))
                {
                    var raceJSON = JsonConvert.DeserializeObject<List<DnDRace>>(sr.ReadToEnd());

                    foreach (var r in raceJSON.Where(z => !z.Name.Contains("base")))
                    {
                        Races.Add(r);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private static void initFeatures()
        {
            try
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
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private static void initNames()
        {
            try
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
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private static void initSkills()
        {
            try
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
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }

    public class DnDRace
    {
        public string Name;
        public string Size;
        public int Speed;
        public List<string> Languages = new List<string>();
        public List<DnDFeature> RacialFeatures = new List<DnDFeature>();
        public Dictionary<string, int> AbilityChanges = new Dictionary<string, int>();

        public DnDRace(string name, string size, int speed, List<string> languages, List<string> racialFeatures)
        {
            Name = name;
            Size = size;
            Speed = speed;
            Languages = languages.ToList();

            foreach (string thing in racialFeatures)
            {
                RacialFeatures.Add(DnDCore.Features.Find(z => z.Name.Equals(thing)));
            }
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
        public string Description;
        public int HitDieType;
        public List<string> PrimaryAbilities = new List<string>();
        public List<string> SaveProficiencies = new List<string>();
        public List<string> GearProficiencies = new List<string>();
        public Dictionary<string, int> Progression = new Dictionary<string, int>();
        public int numSkills;
        public List<string> ClassSkills = new List<string>();

        public override string ToString()
        {
            string outclass = $"{Name}\n{Description}\nHit Die: {HitDieType}\nPrimary Abilities:";
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
            outclass = outclass.Remove(outclass.Length - 1, 1) + $"\nClass Skills - {numSkills} from the following:";
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
            return temp;
        }
    }

    public class DnDFeature
    {
        public string Name;
        public string Description;

        public DnDFeature(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public override string ToString()
        {
            return $"{Name}\n\t{Description}\n";
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

    }

    public class DnDCharacter
    {
        private static Random rand = new Random();

        public string Name = "";

        public int XP = 0;
        public bool isMale = true;
        public DnDRace Race = null;
        public DnDBackground Background;
        public DnDClass Class = null;
        public int Level = 1;
        public int ProficiencyBonus = 0;
        public int AC = 0;
        public int Initiative = 0;
        public int Speed = 0;
        public string GearProficiencies = "";
        public List<DnDFeature> ClassFeatures = new List<DnDFeature>();
        public List<DnDFeature> RacialFeatures = new List<DnDFeature>();
        public List<DnDSkill> Skills = new List<DnDSkill>();
        public int HitDice = 0;

        public Dictionary<string, int> Attributes = new Dictionary<string, int>();
        public Dictionary<string, int> AttributeMods = new Dictionary<string, int>();
        public Dictionary<string, int> Saves = new Dictionary<string, int>();

        public DnDCharacter()
        {
            pickSex();
            createName();

            setRace();
            setClass();
            setAttributes();
            setSaves();

            ProficiencyBonus = 1 + (int)Math.Ceiling(Level * .25);

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
                AttributeMods[attr] = AttrMod(Attributes[attr]);
            }
        }

        private void setClass()
        {
            Class = DnDCore.Classes[rand.Next(DnDCore.Classes.Count)];

            foreach (var f in Class.Progression)
            {
                if (Level >= f.Value)
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
            var verbose = DnDCore.verbose;
            string output = $"{Name}\nLevel {Level} {Race.Name} {Class.Name}\nProficiency Bonus: +{ProficiencyBonus}\n\n";
            foreach (var kvp in Attributes)
            {
                output += $"{kvp.Key}  {kvp.Value},  mod {AttributeMods[kvp.Key]}\n";
            }

            output += $"\nRacial Features\n";
            foreach (var kvp in RacialFeatures)
            {
                output += verbose ? $"\n{kvp.Name}\n\t{kvp.Description}\n" : $"{kvp.Name}, ";
            }
            if (!verbose) { output = output.Remove(output.Length - 2, 2); }

            output += $"\nClass Features\n";
            foreach (var kvp in ClassFeatures)
            {
                output += verbose ? $"\n{kvp.Name}\n\t{kvp.Description}\n" : $"{kvp.Name}, ";
            }
            if (!verbose) { output = output.Remove(output.Length - 2, 2); }





            return output;
        }
    }

}