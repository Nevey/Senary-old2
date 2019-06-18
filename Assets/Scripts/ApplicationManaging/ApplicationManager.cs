using System;
using System.Collections.Generic;
using DependencyInjection.Layers;
using UnityEngine.SceneManagement;
using Utilities;

namespace ApplicationManaging
{
    public static class ApplicationManager
    {
        private static Dictionary<Type, InjectionLayer> injectionLayers = new Dictionary<Type, InjectionLayer>();
        private static bool isStarted;
        private static ApplicationState currentState;

        public static ApplicationState CurrentState => currentState;

        /// <summary>
        /// Create required injection layers, if it's not created yet
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void CreateRequiredInjectionLayers()
        {
            if (!currentState.UseCustomInjectionLayers)
            {
                return;
            }
            
            for (int i = 0; i < currentState.SelectedInjectionLayers.Length; i++)
            {
                Type type = Type.GetType(currentState.SelectedInjectionLayers[i]);

                if (type == null)
                {
                    throw Log.Exception($"ApplicationState {currentState.name} has no eligible InjectionLayer selected!");
                }

                if (!injectionLayers.ContainsKey(type))
                {
                    injectionLayers[type] = (InjectionLayer)Activator.CreateInstance(type);
                    Log.Write($"Instantiated new <b>{type.Name}</b>");
                }
                else
                {
                    Log.Write($"Using already instantiated <b>{type.Name}</b>");
                }
            }
        }
        
        /// <summary>
        /// Opens the required scenes, closes any scenes not required
        /// </summary>
        private static void OpenAndCloseScenes()
        {
            List<Scene> scenesToClose = new List<Scene>();

            bool newSceneIsAlreadyLoaded = false;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (currentState.Scene.Name.Equals(scene.name))
                {
                    newSceneIsAlreadyLoaded = true;
                    continue;
                }

                scenesToClose.Add(scene);
            }

            if (!newSceneIsAlreadyLoaded)
            {
                SceneManager.LoadSceneAsync(currentState.Scene);
            }

            for (int i = 0; i < scenesToClose.Count; i++)
            {
                SceneManager.UnloadSceneAsync(scenesToClose[i].buildIndex);
            }
        }

        private static void HandleUI()
        {
            // TODO: Add UI System
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newState"></param>
        public static void SetState(ApplicationState newState)
        {
            if (!isStarted)
            {
                throw Log.Exception("Cannot set state if not yet started!");
            }
            
            currentState = newState;
            
            CreateRequiredInjectionLayers();
            OpenAndCloseScenes();
        }

        /// <summary>
        /// Starts the application manager, creates the default injection layer and sets
        /// the initially given application state
        /// </summary>
        /// <param name="initialState"></param>
        public static void Start(ApplicationState initialState)
        {
            isStarted = true;
            
            // Create the default injection layer, in case you don't care about DI layering
            injectionLayers.Add(typeof(GenericInjectionLayer), new GenericInjectionLayer());
            SetState(initialState);
        }
    }
}