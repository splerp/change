﻿using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using MonoJam.Utils;
using System.Collections.Generic;
using System.Linq;

namespace MonoJam.Controllers
{
    public class SoundController
    {
        public const int MAX_INSTANCES_PER_SOUND = 3;

        private MonoJam mj;

        public SoundController(MonoJam mjIn)
        {
            mj = mjIn;
        }

        public static void Play(Sound s, bool loop = false)
        {
            // Remove any that are no longer playing.
            s.instances = s.instances.Where(a => a.State == SoundState.Playing).ToList();

            if (s.instances.Count < MAX_INSTANCES_PER_SOUND)
            {
                var sound = s.data.CreateInstance();
                s.instances.Add(sound);

                sound.Volume = s.volume;
                sound.IsLooped = loop;
                sound.Play();
            }
        }

        public static void Play(Sound[] sounds, bool loop = false)
        {
            Play(sounds.RandomElement());
        }

        public static void Stop(Sound s)
        {
            foreach(var instance in s.instances)
            {
                instance.Stop();
            }
        }

        public static void Stop(Sound[] sounds)
        {
            foreach (var s in sounds)
            {
                Stop(s);
            }
        }

        public static void StopAllLoops()
        {
            foreach (var s in Sound.AllSounds)
            {
                foreach(var i in s.instances)
                {
                    if(i.IsLooped)
                    {
                        i.Stop();
                    }
                }
            }
        }
    }

    public class Sound
    {
        public string audioName;
        public float volume;
        public SoundEffect data;
        public List<SoundEffectInstance> instances;

        private Sound(string audioNameIn, float volumeIn = 1f)
        {
            audioName = audioNameIn;
            volume = volumeIn;

            instances = new List<SoundEffectInstance>();

            if (AllSounds == null)
            {
                AllSounds = new List<Sound>();
            }

            AllSounds.Add(this);
        }

        public static void LoadContent(ContentManager cm)
        {
            foreach (var sound in AllSounds)
            {
                sound.data = cm.Load<SoundEffect>("Audio/" + sound.audioName);
            }
        }

        public static List<Sound> AllSounds;

        public static Sound Oink = new Sound("Oinks", 0.5f);
        public static Sound PigDeath = new Sound("PigDeath", 0.5f);
        public static Sound LaserStart = new Sound("LaserStart", 0.2f);
        public static Sound LaserLoop = new Sound("LaserLoop", 0.2f);
        public static Sound LaserEnd = new Sound("LaserEnd", 0.2f);
        public static Sound Boom1 = new Sound("Boom1");
        public static Sound Boom2 = new Sound("Boom2");
        public static Sound Boom3 = new Sound("Boom3");
        public static Sound Boom4 = new Sound("Boom4");
        public static Sound FireFlare = new Sound("FireFlare", 0.7f);
        public static Sound Ding = new Sound("Ding", 0.5f);
        public static Sound Bip1 = new Sound("Bip1", 0.2f);
        public static Sound Bip2 = new Sound("Bip2", 0.2f);
        public static Sound CoinsDrop = new Sound("CoinsDrop", 0.4f);
        public static Sound Slurp1 = new Sound("Slurp1", 0.5f);
        public static Sound Slurp2 = new Sound("Slurp2", 0.5f);
        public static Sound Slurp3 = new Sound("Slurp3", 0.5f);

        public static Sound[] Boom = new Sound[]
        {
            Boom1, Boom2, Boom3, Boom4
        };

        public static Sound[] Slurp = new Sound[]
        {
            Slurp1, Slurp2, Slurp3
        };
    }
}
