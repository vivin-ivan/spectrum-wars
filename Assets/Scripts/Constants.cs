namespace Spectrum.Constants
{
    abstract class Tag
    {
        public const string LocalVRSystem = "Local VR System";
        public const string PlayerHead = "Player Head";
        public const string Target = "Target";
        public const string NetworkManager = "Network Manager";
    }

    abstract class Scene
    {
        public const string MainMenu = "MainMenu";
        public const string Game = "Game";
    }

    abstract class SpawnTypeIndex
    {
        public const int Lobby = 0;
        public const int Game = 1;
    }
}
