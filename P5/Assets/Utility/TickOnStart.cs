using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class TickOnStart : MonoBehaviour
    {
        public static string s_DateTimeNowTicks;
        void Start()
        {
            s_DateTimeNowTicks = DateTime.Now.Ticks.ToString();
        }
    }
}
