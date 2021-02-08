﻿using System.Linq;
using System.Threading.Tasks;
using Blazeroids.Core;
using Blazeroids.Core.Components;
using Blazor.Extensions;

namespace Blazeroids.Web.Game.Components
{
    public class BulletBrain : BaseComponent
    {
        private readonly MovingBody _movingBody;
        private readonly TransformComponent _transformComponent;
        private readonly BoundingBoxComponent _boundingBox;
        
        public BulletBrain(GameObject owner) : base(owner)
        {
            _movingBody = owner.Components.Get<MovingBody>();
            _transformComponent = owner.Components.Get<TransformComponent>();
            _boundingBox = owner.Components.Get<BoundingBoxComponent>();
            _boundingBox.OnCollision += (sender, collidedWith) =>
            {
                if (collidedWith.Owner.Components.TryGet<AsteroidBrain>(out var _))
                    this.Owner.Enabled = false;
            };
        }

        public override async ValueTask Update(GameContext game)
        {
            _movingBody.Thrust = this.Speed;

            var isOutScreen = _transformComponent.World.Position.X < 0 ||
                              _transformComponent.World.Position.Y < 0 ||
                              _transformComponent.World.Position.X > this.Canvas.Width ||
                              _transformComponent.World.Position.Y > this.Canvas.Height;
            if (isOutScreen)
                this.Owner.Enabled = false; 
        }

        public float Speed { get; set; }
        public BECanvasComponent Canvas { get; set; }
    }
}