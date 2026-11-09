using Agate.MVC.Base;

namespace ProjectTA.Module.Tutorial
{
    public class TutorialController : ObjectController<TutorialController, TutorialView>
    {
        private int _currentIndex = 0;

        public override void SetView(TutorialView view)
        {
            base.SetView(view);

            view.SetCallbacks(OnNext, OnPrevious);
            UpdateContent();
        }

        private void OnNext()
        {
            _currentIndex++;
            _currentIndex = _currentIndex >= _view.TutorialItems.Count ? 0 : _currentIndex;
            UpdateContent();
        }

        private void OnPrevious()
        {
            _currentIndex--;
            _currentIndex = _currentIndex < 0 ? _view.TutorialItems.Count - 1 : _currentIndex;
            UpdateContent();
        }

        private void UpdateContent()
        {
            _view.ImageComponent.sprite = _view.TutorialItems[_currentIndex].Image;
            _view.TitleText.SetText(_view.TutorialItems[_currentIndex].Title);
            _view.DescriptionText.SetText(_view.TutorialItems[_currentIndex].Description);
        }
    }
}