using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WickedCrush
{
    public class StateMachine
    {
        //public Dictionary<string, bool> switchbox;
        public State currentControlState;
        public State previousControlState;
        public Dictionary<String, State> control;

        public StateMachine(Dictionary<String, State> ctrl)
        {
            control = ctrl;
            //switchbox = new Dictionary<string, bool>();
            //loadSwitchbox(control);
            currentControlState = null;
            previousControlState = null;
        }

        /*private void loadSwitchbox(List<State> statelist)
        {
            foreach (State st in statelist)
            {
                switchbox.Add(st.name, false);
            }
        }

        private void wipeSwitchbox()
        {
            foreach (string key in switchbox.Keys)
            {
                switchbox[key] = false;
            }
        }*/

        public void Update(GameTime gameTime, Character ch)
        {
            updateControlState(gameTime, ch);
            executeControlState(gameTime, ch);
        }

        private void updateControlState(GameTime gameTime, Character ch)
        {
            foreach (KeyValuePair<String, State> st in control)
            {
                if (st.Value.testCondition(ch))
                {
                    currentControlState = st.Value;
                    return;
                }
            }
        }

        private void executeControlState(GameTime gameTime, Character ch)
        {
            currentControlState.runAction(ch);
            previousControlState = currentControlState;
        }

    }
}
