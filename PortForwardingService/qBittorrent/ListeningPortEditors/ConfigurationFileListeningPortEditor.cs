#nullable enable

using IniParser;
using IniParser.Model;
using IniParser.Model.Configuration;
using IniParser.Parser;
using System;
using System.Text;
using System.Threading.Tasks;

namespace PortForwardingService.qBittorrent.ListeningPortEditors;

internal class ConfigurationFileListeningPortEditor: ListeningPortEditor {

    private const string SECTION_NAME              = "Preferences";
    private const string LISTENING_PORT_ENTRY_NAME = @"Connection\PortRangeMin";

    private static readonly string CONFIGURATION_FILE_PATH = Environment.ExpandEnvironmentVariables(@"%appdata%\qBittorrent\qBittorrent.ini");

    private readonly FileIniDataParser iniFileEditor = new(new IniDataParser(new IniParserConfiguration { AssigmentSpacer = string.Empty }));

    public Task setListeningPort(ushort listeningPort) {
        IniData configContents = readConfigurationFile();

        configContents[SECTION_NAME][LISTENING_PORT_ENTRY_NAME] = Convert.ToString(listeningPort);

        iniFileEditor.WriteFile(CONFIGURATION_FILE_PATH, configContents);
        Console.WriteLine($"Set qBittorrent listening port to {listeningPort} using configuration file.");

        return Task.CompletedTask;
    }

    public Task<ushort?> getListeningPort() {
        IniData configContents = readConfigurationFile();
        return Task.FromResult<ushort?>(Convert.ToUInt16(configContents[SECTION_NAME][LISTENING_PORT_ENTRY_NAME]));
    }

    private IniData readConfigurationFile() {
        return iniFileEditor.ReadFile(CONFIGURATION_FILE_PATH, Encoding.UTF8);
    }

}