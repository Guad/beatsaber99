using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaber99Client
{
    public class Executor : MonoBehaviour
    {
        public static void Init()
        {
            new GameObject("tcp_server_executor").AddComponent<Executor>();
        }

        private static ConcurrentQueue<Func<Task>> _actions = new ConcurrentQueue<Func<Task>>();

        public static void Enqueue(Func<Task> action)
        {
            _actions.Enqueue(action);
        }

        public async void Update()
        {
            while (_actions.Count > 0)
            {
                if (_actions.TryDequeue(out var result))
                    await result();
                    
            }
        }

        public void Start()
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}