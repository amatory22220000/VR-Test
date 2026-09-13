namespace MeowStudio.Data
{
    public class GameSettingsProvider
    {
		public GameSettings settings { get; private set; }
		public GameSettingsProvider(GameSettings settings) => this.settings = settings;
	}
}

