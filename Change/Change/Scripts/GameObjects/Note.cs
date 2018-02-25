﻿using Microsoft.Xna.Framework;
using Splerp.Change.Controllers;
using System.Collections.Generic;

namespace Splerp.Change.GameObjects
{
    public sealed class Note : GameObject, ICollisionObject, IHurtable
    {
        public enum NoteType { None, Pink5, Blue10, Red20, Yellow50, Green100 }

        // Map the note types to their monetary value (in cents).
        public static Dictionary<NoteType, int> noteWorths = new Dictionary<NoteType, int>
        {
            {NoteType.None, 0 },
            {NoteType.Pink5, 500 },
            {NoteType.Blue10, 1000 },
            {NoteType.Red20, 2000 },
            {NoteType.Yellow50, 5000 },
            {NoteType.Green100, 10000 }
        };

        // Map the note types to their spawn probabilities.
        public static Dictionary<NoteType, int> noteSpawnWeights = new Dictionary<NoteType, int>
        {
            {NoteType.None, 0 },
            {NoteType.Pink5, 150 },
            {NoteType.Blue10, 15 },
            {NoteType.Red20, 7 },
            {NoteType.Yellow50, 5 },
            {NoteType.Green100, 1 }
        };

        public const int WIDTH = 8;
        public const int HEIGHT = 4;

        private GameController gc;

        public Point Size => new Point(WIDTH, HEIGHT);
        public Rectangle CollisionRect => new Rectangle(Position.ToPoint(), Size);

        // Calculated properties.
        public bool InRangeForCatching => Position.Y < ChangeGame.PLAYABLE_AREA_HEIGHT;
        public bool ReadyToRemove => (Position.Y > ChangeGame.PLAYABLE_AREA_HEIGHT + ChangeGame.PADDLE_AREA_HEIGHT) || (CaughtByPlayer && Position.Y > ChangeGame.PLAYABLE_AREA_HEIGHT);
        public bool CaughtByPlayer;

        // Used to track if this note is caught by a vacuum, and if it's currently inside one.
        public VacuumEnemy CaughtByVacuum;
        public bool InsideVacuum;

        // How quickly the note moves.
        public float downSpeed = 0.1f;

        // For how long the note should be invulnerable.
        // When a player kills a vacuum, the released notes shouldn't die on the next frame.
        public int invulnCount = 100;
        public int invulnCountdown;

        public int MaxHealth => 1;
        public int CurrentHealth { get; set; }
        public bool IsDead => CurrentHealth <= 0;

        public NoteType Type { get; private set; }

        public Note(GameController gcIn, NoteType typeIn)
        {
            gc = gcIn;
            Type = typeIn == NoteType.None ? NoteType.Pink5 : typeIn;

            // Pick a random spawn point.
            SetX(GameController.random.Next(0, ChangeGame.WINDOW_WIDTH - WIDTH));
            SetY(-HEIGHT);

            CurrentHealth = MaxHealth;
        }

        public void Update()
        {
            if(CaughtByPlayer)
            {
                if (Position.X < gc.paddlePlayer.CollisionRect.X + PaddlePlayer.GRAPHIC_EDGE_WIDTH)
                {
                    SetX(gc.paddlePlayer.CollisionRect.X + PaddlePlayer.GRAPHIC_EDGE_WIDTH);
                }
                else if (Position.X + WIDTH > gc.paddlePlayer.CollisionRect.Right - PaddlePlayer.GRAPHIC_EDGE_WIDTH)
                {
                    SetX(gc.paddlePlayer.CollisionRect.Right - PaddlePlayer.GRAPHIC_EDGE_WIDTH - WIDTH);
                }

                MoveBy(new Vector2(0, downSpeed));
            }
            else if(CaughtByVacuum != null)
            {
                SetX(CaughtByVacuum.MouthPosX);

                // Move towards the vacuum.
                if (CollisionRect.Y < CaughtByVacuum.CollisionRect.Y)
                {
                    MoveBy(new Vector2(0, 1));
                }
                else if (CollisionRect.Y > CaughtByVacuum.CollisionRect.Y + VacuumEnemy.HEIGHT)
                {
                    MoveBy(new Vector2(0, -1));
                }
                else
                {
                    //Caught.
                    InsideVacuum = true;
                    invulnCountdown = invulnCount;
                }
            }
            else
            {
                MoveBy(new Vector2(0, downSpeed));
            }

            invulnCountdown--;
        }

        public void Damage(int amount)
        {
            if(!InsideVacuum && invulnCountdown <= 0)
            {
                CurrentHealth -= amount;
            }
        }
    }
}
