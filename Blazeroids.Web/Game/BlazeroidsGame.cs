using System;
using System.Drawing;
using System.Threading.Tasks;
using Blazeroids.Core;
using Blazeroids.Core.Assets;
using Blazeroids.Core.Components;
using Blazeroids.Web.Game.Components;
using Blazeroids.Web.Game.GameObjects;
using Blazor.Extensions;
using Blazeroids.Core.Utils;

namespace Blazeroids.Web.Game
{
    public class BlazeroidsGame : GameContext
    {
        private readonly BECanvasComponent _canvas;
        private readonly IAssetsResolver _assetsResolver;
        
        public BlazeroidsGame(BECanvasComponent canvas, IAssetsResolver assetsResolver)
        {
            _canvas = canvas;
            _assetsResolver = assetsResolver;
        }

        protected override async ValueTask Init()
        {   
            this.AddService(new InputService());

            var collisionService = new CollisionService(this, new Size(64, 64));
            this.AddService(collisionService);
            
            var sceneGraph = new SceneGraph(this);
            this.AddService(sceneGraph);
            
            //var fpsCounter = new GameObject();
            //fpsCounter.Components.Add<FPSCounterComponent>();
            //game._sceneGraph.Root.AddChild(fpsCounter);

            var bulletSpawner = BuildBulletSpawner();
            sceneGraph.Root.AddChild(bulletSpawner);
            
            var player = BuildPlayer(bulletSpawner);
            sceneGraph.Root.AddChild(player);

            for (var i = 0; i != 6; ++i)
                AddAsteroid(sceneGraph);

            var context = await _canvas.CreateCanvas2DAsync();
            var renderService = new RenderService(this, context);
            this.AddService(renderService);
        }

        private Spawner BuildBulletSpawner()
        {
            var spriteSheet = _assetsResolver.Get<SpriteSheet>("assets/sheet.json");
            
            var spawner = new Spawner(() =>
            {
                var bullet = new GameObject();
                bullet.Components.Add<TransformComponent>();

                var bulletSpriteRenderer = bullet.Components.Add<SpriteRenderComponent>();
                bulletSpriteRenderer.Sprite = spriteSheet.Get("fire01.png");

                var bulletBBox = bullet.Components.Add<BoundingBoxComponent>();
                bulletBBox.SetSize(bulletSpriteRenderer.Sprite.Bounds.Size);

                var speed = 7000f;
                
                var bulletRigidBody = bullet.Components.Add<MovingBody>();
                bulletRigidBody.MaxSpeed = speed;

                var brain = bullet.Components.Add<BulletBrain>();
                brain.Speed = speed;
                brain.Canvas = _canvas;

                return bullet;
            }, bullet =>
            {
                bullet.Components.Get<MovingBody>().Reset();
                
                bullet.Components.Get<TransformComponent>().Local.Reset();
                bullet.Components.Get<TransformComponent>().World.Reset();
            });

            spawner.Components.Add<TransformComponent>();

            return spawner;
        }

        private GameObject BuildPlayer(Spawner bulletSpawner)
        {
            var player = new GameObject();

            var spriteSheet = _assetsResolver.Get<SpriteSheet>("assets/sheet.json");
            var sprite = spriteSheet.Get("playerShip2_green.png");

            var playerTransform = player.Components.Add<TransformComponent>();

            playerTransform.Local.Position.X = _canvas.Width / 2;
            playerTransform.Local.Position.Y = _canvas.Height / 2;

            var playerSpriteRenderer = player.Components.Add<SpriteRenderComponent>();
            playerSpriteRenderer.Sprite = sprite;

            var bbox = player.Components.Add<BoundingBoxComponent>();
            bbox.SetSize(sprite.Bounds.Size);

            var rigidBody = player.Components.Add<MovingBody>();
            rigidBody.MaxSpeed = 400f;

            var weapon = player.Components.Add<Weapon>();
            weapon.Spawner = bulletSpawner;

            player.Components.Add<PlayerBrain>();
            
            return player;
        }

        private void AddAsteroid(SceneGraph sceneGraph)
        {
            var asteroid = new GameObject();

            var spriteSheet = _assetsResolver.Get<SpriteSheet>("assets/sheet.json");
            var sprite = spriteSheet.Get("meteorBrown_big1.png");
            
            var transform = asteroid.Components.Add<TransformComponent>();
           
            var w = (double)_canvas.Width;
            var rx = MathUtils.Random.NextDouble(0, .35, .65, 1);
            var tx = MathUtils.Normalize(rx, 0, 1, -1, 1);
            transform.Local.Position.X = (float)(tx * w/2 + w/2);

            var h = (double)_canvas.Height;
            var ry = MathUtils.Random.NextDouble(0, .35, .65, 1);
            var ty = MathUtils.Normalize(ry, 0, 1, -1, 1);
            transform.Local.Position.Y = (float)(ty * h/2 + h/2);

            
            var spriteRenderer = asteroid.Components.Add<SpriteRenderComponent>();
            spriteRenderer.Sprite = sprite;
            
            var bbox = asteroid.Components.Add<BoundingBoxComponent>();
            bbox.SetSize(sprite.Bounds.Size);

            asteroid.Components.Add<AsteroidBrain>();

            sceneGraph.Root.AddChild(asteroid);
        }

    }
}