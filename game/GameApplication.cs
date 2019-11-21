using DefaultEcs;
using game.ECS.Events;
using game.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace game
{
    public class GameApplication : Game
    {
        static void Main()
        {
            using var game = new GameApplication();
            game.Run();
        }
        
        private const string StartingMapName = @"hub";
        private const string StartingSpawnName = @"spawn01";
        
        private readonly ScreenManager screenManager;

        private GameApplication()
        {
            IsFixedTimeStep = true;
            new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280, PreferredBackBufferHeight = 720
            };

            Content.RootDirectory = "Content";

            screenManager = new ScreenManager(this, 1 << 8);
            screenManager.Subscribe(this);
            Components.Add(screenManager);
            
            screenManager.AddScreen( new SceneScreen(StartingMapName, StartingSpawnName));
            screenManager.AddScreen( new EntryScreen());
        }

        protected override void Initialize()
        {
            base.Initialize();
            Window.Title = "Whack a ...!!!";
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Input.Update(gameTime);
            base.Update(gameTime);
        }
        
        [Subscribe]
        private void OnPlaySound(in PlaySound @event)
        {
            var content = screenManager.Game.Content;
            var soundEffect = content.Load<SoundEffect>(@event.soundName);
            soundEffect.Play();
        }

        [Subscribe]
        private void OnStopSong(in StopSongEvent @event)
        {
            MediaPlayer.Stop();
        }

        [Subscribe]
        private void OnPlaySong(in PlaySongEvent @event)
        {
            var content = screenManager.Game.Content;
            var song = content.Load<Song>(@event.songName);
            MediaPlayer.IsRepeating = @event.isRepeating;
            MediaPlayer.Play(song);
        }
    }
}