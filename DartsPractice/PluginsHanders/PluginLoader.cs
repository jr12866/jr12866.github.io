using DartsPractice.Core.Interfaces;
using System.Reflection;

namespace DartsPractice.PluginsHanders;

public class PluginLoader
{
    public List<IDartsGame> LoadPlugins(string folderPath)
    {
        var games = new List<IDartsGame>();
        if (!Directory.Exists(folderPath)) return  games;

        foreach (string file in  Directory.GetFiles(folderPath, "*.dll"))
        {
            Assembly assembly = Assembly.LoadFrom(file);

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IDartsGame).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    if (Activator.CreateInstance(type) is IDartsGame game)
                    {
                        games.Add(game);
                    }
                }
            }
        }
        return games;
    }
}