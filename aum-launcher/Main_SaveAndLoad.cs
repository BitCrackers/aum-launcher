using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace aum_launcher
{
    public partial class Main
    {
        private void InitializeProfileSystem()
        {
            // upon program start, the Profiles directory is iterated and each file is deserialized into a new Profile instance
            // a step of this deserialization process is deserializing each ModItem, too. and a root node in ProfilesTreeView is created for each unique associated process.
            Profile defaultProfile = new Profile();
            if (Directory.Exists(MOD_FOLDER_NAME))
            {
                Logger.Log.Write(MOD_FOLDER_NAME + " already exists", Logger.ELogType.Info, rtxtLog);
                foreach (string filename in Directory.EnumerateFiles(MOD_FOLDER_NAME, "*?" + PROFILE_FILE_EXT, SearchOption.TopDirectoryOnly))
                {
                    using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (BinaryReader reader = new BinaryReader(fs))
                        {
                            Profile profile = new Profile();
                            if (!profile.Deserialize(reader))
                            {
                                Logger.Log.WriteError("Error loading profile '" + filename + "'", rtxtLog);
                            }
                            // load GUI controls state
                            ActiveProfile = profile;
                            Logger.Log.Write("Loaded profile from '" + filename + "'", Logger.ELogType.Info, rtxtLog);
                            return;
                        }
                    }
                }
            }
            else
            {
                // NOTE:
                // first run of program
                Directory.CreateDirectory(MOD_FOLDER_NAME);
                Logger.Log.Write(MOD_FOLDER_NAME + " did not exist, created it for subsequent launches", Logger.ELogType.Info, rtxtLog);
            }
            ActiveProfile = defaultProfile;
            Logger.Log.Write("Loaded default profile", Logger.ELogType.Info, rtxtLog);
        }

        private void SaveProfileSystem()
        {
            // upon program exit, the AllProfiles collection is iterated and serialized to files (OverwriteNew)
            string filePath = ActiveProfile.GetFilePath();
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    if (!ActiveProfile.Serialize(writer))
                    {
                        Logger.Log.WriteError("Error saving profile to '" + filePath + "'", rtxtLog, true);
                        return;
                    }
                }
            }
            Logger.Log.Write("Successfully saved profile to '" + filePath + "'", Logger.ELogType.Info, rtxtLog, true);
        }
    }
}
