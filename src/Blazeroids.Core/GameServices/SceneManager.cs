using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blazeroids.Core.GameServices
{
    public abstract class Scene
    {
        protected GameContext Game { get; }

        protected Scene(GameContext game)
        {
            this.Game = game ?? throw new ArgumentNullException(nameof(game));
        }

        public async ValueTask Step()
        {
            if (null != Root)
                await Root.Update(this.Game);
            await this.Update();
        }

        public virtual ValueTask Enter() => ValueTask.CompletedTask;
        public virtual ValueTask Exit() => ValueTask.CompletedTask;
        protected virtual ValueTask Update() => ValueTask.CompletedTask;

        public GameObject Root { get; } = new();
    }

    public class SceneManager : IGameService
    {
        private readonly GameContext _game;
        private readonly Dictionary<string, Scene> _scenes = new();

        public SceneManager(GameContext game)
        {
            _game = game;
        }

        public void AddScene(string name, Scene scene)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (null == scene)
                throw new ArgumentNullException(nameof(scene));
            _scenes.Add(name, scene);
        }

        public async ValueTask SetCurrentScene(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (!_scenes.ContainsKey(name))
                throw new ArgumentOutOfRangeException(nameof(name), $"invalid scene name: '{name}'");
            if (this.Current is not null)
                await this.Current.Exit();

            this.Current = _scenes[name];

            await this.Current.Enter();

            this.OnSceneChanged?.Invoke(this.Current);
        }

        public async ValueTask Step()
        {
            if (this.Current is not null)
                await this.Current.Step();
        }

        public Scene Current { get; private set; }

        public event OnSceneChangedHandler OnSceneChanged;
        public delegate void OnSceneChangedHandler(Scene currentScene);
    }
}