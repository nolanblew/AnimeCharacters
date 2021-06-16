using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;

namespace AnimeCharacters.Pages
{
    public abstract class BasePage : ComponentBase
    {
        bool _hasInitializedBeenCalled;
        readonly object _initializeLock = new();

        [Inject]
        protected IDatabaseProvider DatabaseProvider { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }
        
        [Inject]
        protected IPageStateManager PageStateManager { get; set; }

        [Inject]
        protected IEventAggregator _EventAggregator { get; set; }

        protected override void OnInitialized()
        {
            if (!_hasInitializedBeenCalled)
            {
                lock (_initializeLock)
                {
                    if (!_hasInitializedBeenCalled)
                    {
                        _hasInitializedBeenCalled = true;
                        PageStateManager.Add(NavigationManager.Uri);
                        _EventAggregator.Subscribe(this);
                    }
                }
            }

            base.OnInitialized();
        }
    }
}
