using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using WickedCrush.Utility;

namespace WickedCrush
{
    public class CharacterFactory //used to facilitate characters adding more characters
    {
        public ContentManager _cm;
        public GraphicsDevice _gd; //singleton
        public SoundManager _sound;

        public List<Character> characterListQueue; //wait here
        public List<PointLight> lightListQueue; //wait here
        public List<DamageNumber> damageListQueue;

        public List<Character> _characterList; //singleton i think
        public List<PointLight> _lightList;
        public List<DamageNumber> _damageNumbersList;

        public CharacterFactory(List<Character> characterList, List<PointLight> lightList, List<DamageNumber> damageNumbersList, Overlord overlord)
        {
            characterListQueue = new List<Character>();
            lightListQueue = new List<PointLight>();
            damageListQueue = new List<DamageNumber>();

            _characterList = characterList;
            _lightList = lightList;
            _damageNumbersList = damageNumbersList;
            _gd = overlord._gd;
            _cm = overlord._cm;
            _sound = overlord._sound;
        }

        public void AddCharacterToList(Character c)
        {
            characterListQueue.Add(c);
        }

        public void AddLightToList(PointLight l)
        {
            lightListQueue.Add(l);
        }

        public void ProcessQueue()
        {
            foreach (Character c in characterListQueue)
            {
                _characterList.Add(c);
            }
            characterListQueue.Clear();

            foreach (PointLight l in lightListQueue)
            {
                _lightList.Add(l);
            }
            lightListQueue.Clear();

            foreach (DamageNumber d in damageListQueue)
            {
                _damageNumbersList.Add(d);
            }
            damageListQueue.Clear();
        }
    }
}
