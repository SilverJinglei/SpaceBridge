using System;
using System.Windows;
using System.Windows.Interactivity;

namespace Military.Wpf.Utility.Behavior
{
    /// <summary>
    ///<s:SurfaceSlider Name=”timelineSlider” Grid.Row=”1″>
    /// <i:Interaction.Triggers>
    ///   <helpers:RoutedEventTrigger RoutedEvent=”Thumb.DragStarted” >
    ///    <cmd:EventToCommand Command=”{Binding Mode=OneWay, Path=SeekStartedCmd}”
    ///      PassEventArgsToCommand=”True” />
    ///  </i:Interaction.Triggers>
    ///</s:SurfaceSlider>
    /// </summary>
    public class RoutedEventTrigger : EventTriggerBase<DependencyObject>
    {
        public RoutedEvent RoutedEvent { get; set; }

        protected override void OnAttached()
        {
            var behavior = base.AssociatedObject as System.Windows.Interactivity.Behavior;
            FrameworkElement associatedElement = base.AssociatedObject as FrameworkElement; if (behavior != null)
            {
                associatedElement = ((IAttachedObject)behavior).AssociatedObject as FrameworkElement;
            }
            if (associatedElement == null)
            {
                throw new ArgumentException("Routed Event trigger can only be associated to framework elements");
            }
            if (RoutedEvent != null)
            {
                associatedElement.AddHandler(RoutedEvent, new RoutedEventHandler(this.OnRoutedEvent));
            }
        }

        void OnRoutedEvent(object sender, RoutedEventArgs args)
        {
            base.OnEvent(args);
        }

        protected override string GetEventName()
        {
            return RoutedEvent.Name;
        }
    }
}
