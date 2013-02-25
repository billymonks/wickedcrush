using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace WickedCrush.Utility
{
    public class SoundManager
    {
        private Dictionary<String, SoundEffect> soundsList;
        private Dictionary<int, SoundEffectInstance> loopingSoundsList;

        private ContentManager _cm;
        private Camera _gameCam;

        //private Vector3 listenerPos;

        public AudioListener listener;

        int i;

        public SoundManager(ContentManager cm)
        {
            soundsList = new Dictionary<String, SoundEffect>();
            loopingSoundsList = new Dictionary<int, SoundEffectInstance>();

            SoundEffect.DistanceScale = 300f;

            _cm = cm;
            
            i = 0;
        }

        public void Update(GameTime gameTime)
        {
            if (_gameCam != null && listener!=null)
            {
                listener.Position = _gameCam.cameraPosition;
            }
        }

        public void setCam(Camera gameCam)
        {
            _gameCam = gameCam;

            listener = new AudioListener();
            listener.Position = gameCam.cameraPosition;
        }

        public void clearList()
        {
            soundsList.Clear();
        }

        public void addSound(String key, String name)
        {
            if (!soundsList.ContainsKey(key))
            {
                soundsList.Add(key, _cm.Load<SoundEffect>(@"sounds/"+name));
            }
        }

        public void playSound(String key)
        {
            if (soundsList.ContainsKey(key))
            {
                soundsList[key].Play();
            }
        }

        public void playAmbientLoop(String key)
        {
            if (soundsList.ContainsKey(key))
            {
                loopingSoundsList.Add(i, soundsList[key].CreateInstance());
                loopingSoundsList[i].IsLooped = true;
                loopingSoundsList[i].Volume = .3f;
                loopingSoundsList[i].Play();

                i++;
            }
        }

        public void stopAmbientLoops()
        {
            for (int j = i; j >= 0; j--)
            {
                if (loopingSoundsList.ContainsKey(j))
                {
                    loopingSoundsList[j].Stop();
                    loopingSoundsList.Remove(j);
                }
            }
        }

        public SoundEffectInstance getSoundInstance(String key)
        {
            if (soundsList.ContainsKey(key))
            {
                return soundsList[key].CreateInstance();
            }
            else return null;
        }
    }
}
