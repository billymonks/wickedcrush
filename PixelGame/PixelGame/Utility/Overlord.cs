using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WickedCrush.GameStates;

namespace WickedCrush.Utility
{
    //controls game flow, holds important things
    //lets me manipulate things lazily
    //probably bad but I know not what I do
    public class Overlord
    {
        public ContentManager _cm;
        public GraphicsDevice _gd;
        public SoundManager _sound;
        public Stack<GameState> _cGameState;

        public Gameplay _gameplay;
        public Editor _editor;

        public User currentUser;

        public bool editorMenuOpen;
        public bool isOrtho;

        public Overlord(ContentManager cm, GraphicsDevice gd) // for the editor
        {
            _cm = cm;
            _gd = gd;
            isOrtho = true;
        }
        
        public Overlord(ContentManager cm, GraphicsDevice gd, SoundManager sound, Stack<GameState> cGameState)
        {
            _cm = cm;
            _gd = gd;
            _sound = sound;
            _cGameState = cGameState;
            isOrtho = true;
        }

        public void returnToLevelMenu()
        {
            _gameplay.StopCharacterSoundInstances();
            _sound.stopAmbientLoops();
            _cGameState.Pop();
        }

        public void LoadLevel(String s)
        {
            _gameplay.InitializeLevelLoad(s);
            _cGameState.Push(GameState.InGame);
        }

        public void LaunchEditor()
        {
            _editor.NewLevel();
            _cGameState.Push(GameState.Editor);
        }

        public void popGameState()
        {
            if (_cGameState.Peek().Equals(GameState.InGame))
            {
                _gameplay.StopCharacterSoundInstances();
                _sound.stopAmbientLoops();
            }
            if (_cGameState.Peek().Equals(GameState.Editor))
            {
                _editor.StopEditor();
            }
            _cGameState.Pop();
        }
    }
}
