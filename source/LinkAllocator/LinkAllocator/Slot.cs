﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkAllocator
{
    public class Slot
    {
        public State state { get; private set; }

        /// <summary>
        /// slot owner can be not null only when state is TAKEN
        /// (and this class takes care of it)
        /// </summary>
        public Link slotOWner { get; private set; }
        public string constraintName { get; private set; }

        public enum State
        {
            FREE, TAKEN, FORBIDDEN
        }

        public Slot()
        {
            state = State.FREE;
        }

        public bool IsAvailable()
        {
            return state == State.FREE;
        }

        public void Allocate(Link link)
        {
            if (state != State.FREE)
                throw new ApplicationException("Cannot reserve not free slot");

            state = State.TAKEN;
            slotOWner = link;
        }
        
        /// <summary>
        /// only taken (not forbidden) slots can be deallocated
        /// </summary>
        /// <param name="link"></param>
        public void Deallocate()
        {
            if (state != State.TAKEN)
                throw new ApplicationException("cannot free not taken slot!");

            slotOWner = null;
            state = State.FREE;
        }

        public void Forbid(string _constraintName)
        {
            if (state != State.FREE)
                throw new ApplicationException("Cannot forbid not free slot");

            state = State.FORBIDDEN;
            constraintName = _constraintName;
        }

        public override string ToString()
        {
            string ownerString = slotOWner != null ? " : " + slotOWner.ToString() : String.Empty;

            return state.ToString() + ownerString;
        }
    }
}
