namespace Tesserae.Components
{
    public static class ModalExtensions
    {
        public static T ShowCloseButton<T>(this T modal) where T : Modal
        {
            modal.ShowCloseButton = true;
            return modal;
        }

        public static T HideCloseButton<T>(this T modal) where T : Modal
        {
            modal.ShowCloseButton = false;
            return modal;
        }

        public static T LightDismiss<T>(this T modal) where T : Modal
        {
            modal.CanLightDismiss = true;
            return modal;
        }

        public static T NoLightDismiss<T>(this T modal) where T : Modal
        {
            modal.CanLightDismiss = false;
            return modal;
        }

        public static T Dark<T>(this T modal) where T : Modal
        {
            modal.IsDark = true;
            return modal;
        }

        public static T Draggable<T>(this T modal) where T : Modal
        {
            modal.IsDraggable = true;
            return modal;
        }

        public static T NonBlocking<T>(this T modal) where T : Modal
        {
            modal.IsNonBlocking = true;
            return modal;
        }

        public static T Blocking<T>(this T modal) where T : Modal
        {
            modal.IsNonBlocking = false;
            return modal;
        }
    }
}