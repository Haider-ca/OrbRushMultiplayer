using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbRush.Utilities
{
    // Author: Team
    // Responsibility: Run network updates on Unity main thread
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> Actions = new();

        public static void Enqueue(Action action)
        {
            lock (Actions)
            {
                Actions.Enqueue(action);
            }
        }

        private void Update()
        {
            lock (Actions)
            {
                while (Actions.Count > 0)
                {
                    Actions.Dequeue()?.Invoke();
                }
            }
        }
    }
}