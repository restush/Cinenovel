using Naninovel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace AmoyFeels.Cinenovel
{
    [InitializeAtRuntime]
    public class CinenovelManager : IEngineService
    {

        public readonly struct RuntimeCine : System.IDisposable
        {

            public RuntimeCine(PlayableDirector playableDirector)
            {
                PlayableDirector = playableDirector;
                GameObject = playableDirector.gameObject;
                UniTaskCompletionSource = new UniTaskCompletionSource();
            }

            public readonly PlayableDirector PlayableDirector;
            public readonly GameObject GameObject;
            public readonly UniTaskCompletionSource UniTaskCompletionSource;
            public UniTask Task => UniTaskCompletionSource.Task;
            public bool IsValid() => ObjectUtils.IsValid(PlayableDirector);
            public bool IsValidAndIsNotCompleted() => IsValid() && UniTaskCompletionSource != null && !UniTaskCompletionSource.Task.IsCompleted;

            public void Dispose()
            {
                ObjectUtils.DestroyOrImmediate(GameObject);
            }

            public void TrySetResult() => UniTaskCompletionSource?.TrySetResult();

            public void PlayCine(float startTime, AsyncToken asyncToken)
            {
                PlayCineAync(startTime, asyncToken).Forget();
            }

            async UniTask PlayCineAync(float startTime, AsyncToken asyncToken)
            {
                PlayableDirector.time = startTime;

                PlayableDirector.Play();
                while (Application.isPlaying)
                {
                    if (Task.Status != Naninovel.Async.AwaiterStatus.Pending)
                        break;
                    await UniTask.Yield(asyncToken: asyncToken);
                    if (asyncToken.Completed || asyncToken.Canceled)
                        break;
                    if (!ObjectUtils.IsValid(PlayableDirector) || !PlayableDirector.playableGraph.IsValid())
                        break;

                    if (asyncToken.Canceled || asyncToken.Completed)
                        break;
                    if (PlayableDirector.state != PlayState.Playing)
                        break;
                }
                Dispose();
                if (Task.Status == Naninovel.Async.AwaiterStatus.Pending)
                    TrySetResult();
            }
        }
        public virtual CinenovelConfiguration Configuration { get; }
        public ResourceLoader<GameObject> Loader { get => loader; }

        private readonly IResourceProviderManager providersManager;
        private readonly IStateManager stateManager;
        private readonly IScriptPlayer scriptPlayer;
        private readonly IInputManager inputManager;
        private readonly float defaultSkipSpeed;
        private readonly string[] listBinding;
        private Dictionary<string, RuntimeCine> spawnedCinenovels = new Dictionary<string, RuntimeCine>();
        private ResourceLoader<GameObject> loader;
        private GameObject container;

        public CinenovelManager(CinenovelConfiguration timelineConfiguration, IResourceProviderManager providersManager, IStateManager stateManager, IScriptPlayer scriptPlayer, IInputManager inputManager)
        {
            Configuration = timelineConfiguration;
            this.providersManager = providersManager;
            this.stateManager = stateManager;
            this.scriptPlayer = scriptPlayer;
            this.inputManager = inputManager;
            this.stateManager.OnRollbackStarted += StopAllCines;
            this.scriptPlayer.OnSkip += OnSkip;

            defaultSkipSpeed = this.scriptPlayer.Configuration.SkipTimeScale;
            listBinding = this.inputManager.Configuration.Bindings.Select(x => x.Name).ToArray();
        }

        private void OnSkip(bool obj)
        {
            // When on skip, set skip speed based on Cinenovel Configuration.
            if (obj)
            {
                bool isPlaying = false;
                foreach (var item in spawnedCinenovels)
                {
                    if (item.Value.IsValidAndIsNotCompleted() && item.Value.Task.Status == Naninovel.Async.AwaiterStatus.Pending)
                    {
                        isPlaying = true;
                        break;
                    }
                }

                if (isPlaying)
                    scriptPlayer.Configuration.SkipTimeScale = Configuration.SkipSpeed;
            }
            else
                scriptPlayer.Configuration.SkipTimeScale = defaultSkipSpeed;

        }

        public void DestroyService()
        {
            StopAllCines();
            spawnedCinenovels.Clear();
            loader.ReleaseAll(this);
        }

        public void StopAllCines()
        {
            var cine = spawnedCinenovels.Select(x => x.Key).ToArray();
            foreach (var path in cine)
            {
                StopCine(path,true);
            }
        }

        public UniTask InitializeServiceAsync()
        {
            loader = Configuration.Loader.CreateFor<GameObject>(providersManager);
            container = Engine.CreateObject("Cinenovel");

            Engine.AddPostInitializationTask(LastInitializtion);
            return UniTask.CompletedTask;
        }

        private UniTask LastInitializtion()
        {
            Engine.RemovePostInitializationTask(LastInitializtion);
            return UniTask.CompletedTask;
        }

        public void StopCine(string pathID, bool remove)
        {
            if (spawnedCinenovels.ContainsKey(pathID))
            {
                spawnedCinenovels[pathID].TrySetResult();
                spawnedCinenovels[pathID].Dispose();
                if (remove)
                    loader.Release(pathID, this);
            }
            else
            {
                Debug.LogWarning("There is no `" + pathID + "` cinenovel that currently played");
            }
        }

        public async UniTask PlayCineAsync(string pathId, float startTime = 0, string[] allowedInputSamplers = default, AsyncToken asyncToken = default)
        {
            var allowedBindings = allowedInputSamplers == null || allowedInputSamplers.Length == 0 ? Configuration.allowedInputSamplers :  Configuration.allowedInputSamplers.Union(allowedInputSamplers).ToArray();
            var allBindings = listBinding
                .Where(x => inputManager
                .TryGetSampler(x, out _))
                .Select(name => (name, isEnable: inputManager.GetSampler(name).Enabled, sampler: inputManager.GetSampler(name)))
                .ToArray();

            foreach (var binding in allBindings)
            {
                binding.sampler.Enabled = false;
                if (allowedBindings == null || allowedBindings.Length == 0)
                    continue;
                foreach (var allowedBinding in allowedBindings)
                {
                    if (binding.name.EqualsFast(allowedBinding))
                        binding.sampler.Enabled = true;
                }
            }

            if (StringUtils.IsNullEmptyOrWhiteSpace(pathId))
            {
                Debug.LogWarning("Path id timeline is null or empty");
                return;
            }


            var resource = await Loader.LoadAndHoldAsync(pathId, this);
            PlayableDirector resourcePlayableDirector;
            if (!resource.Valid || !resource.Object.TryGetComponent(out resourcePlayableDirector))
                throw new System.NullReferenceException("Cannot play Cinenovel because of `" + resource.Path + "` is not valid. Make sure path is correct");

            if (!spawnedCinenovels.ContainsKey(pathId))
                spawnedCinenovels.Add(pathId, new RuntimeCine(Engine.Instantiate(resourcePlayableDirector, pathId, 0, container.transform)));
            else
            {
                spawnedCinenovels[pathId].TrySetResult();
                spawnedCinenovels[pathId] = new RuntimeCine(Engine.Instantiate(resourcePlayableDirector, pathId, 0, container.transform));
            }

            spawnedCinenovels[pathId].PlayCine(startTime, asyncToken);
            await spawnedCinenovels[pathId].Task;

            foreach (var binding in allBindings)
                binding.sampler.Enabled = binding.isEnable;
        }

        public void ResetService()
        {
            var spawnedCinenovels = this.spawnedCinenovels.Select(x => x.Key).ToArray();

            foreach (var id in spawnedCinenovels)
            {
                StopCine(id,true);
            }
            this.spawnedCinenovels.Clear();
            loader.ReleaseAll(this);
        }
    }

}