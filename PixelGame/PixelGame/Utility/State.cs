using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WickedCrush
{
    public delegate void ActionDelegate(Object obj); //might change from object to something more specific if it turns out this is bad
    public delegate bool ConditionDelegate(Object obj);

    public class State
    {
        public String name;
        public ActionDelegate action;
        public ConditionDelegate condition;

        public State(String n, ConditionDelegate c, ActionDelegate a)
        {
            name = n;
            condition = c;
            action = a;
        }

        public bool testCondition(Character ch)
        {
            return condition(ch);
        }

        public void runAction(Character ch)
        {
            action(ch);
        }
    }
}
