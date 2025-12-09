using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Amuse.UI.Behaviors
{
    /// <summary>
    /// Behaviour to use Shift + Enfer to add a new line to a TextBox allowing IsDefault Commands to be fired on Enter
    /// </summary>
    public class ShiftEnterBehavior
    {

        /// <summary>
        /// The enable property
        /// </summary>
        public static readonly DependencyProperty EnableProperty = DependencyProperty.RegisterAttached("Enable", typeof(bool), typeof(ShiftEnterBehavior), new PropertyMetadata(false, OnEnableChanged));


        /// <summary>
        /// Gets the enable value.
        /// </summary>
        /// <param name="obj">The object.</param>
        public static bool GetEnable(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableProperty);
        }

        /// <summary>
        /// Sets the enable valse.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetEnable(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableProperty, value);
        }


        /// <summary>
        /// Called when enable changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnEnableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is TextBox textBox)
            {
                bool attach = (bool)e.NewValue;

                if (attach)
                {
                    DataObject.AddPastingHandler(textBox, TextBox_OnPaste);
                    textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
                    textBox.TargetUpdated += TextBox_TargetUpdated;
                }
                else
                {
                    DataObject.RemovePastingHandler(textBox, TextBox_OnPaste);
                    textBox.PreviewKeyDown -= TextBox_PreviewKeyDown;
                    textBox.TargetUpdated -= TextBox_TargetUpdated;
                }
            }
        }


        /// <summary>
        /// Handles the PreviewKeyDown event of the TextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private static void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // If Shift + Enter is pressed append a new line
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Shift && sender is TextBox textBox)
            {
                e.Handled = true;
                using (textBox.DeclareChangeBlock())
                {
                    var caretIndex = textBox.CaretIndex;
                    textBox.Text = textBox.Text.Insert(caretIndex, Environment.NewLine);
                    textBox.CaretIndex = caretIndex + 1;
                }
            }
        }


        /// <summary>
        /// Handles the OnPaste event of the TextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataObjectPastingEventArgs"/> instance containing the event data.</param>
        private static void TextBox_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            // Because AcceptsReturn is false we need to intercept paste to allow new lines
            if (sender is TextBox textBox && e.DataObject.GetDataPresent(DataFormats.UnicodeText) && e.DataObject.GetData(DataFormats.UnicodeText) is string pastedText)
            {
                using (textBox.DeclareChangeBlock())
                {
                    var text = textBox.Text;
                    var caretIndex = textBox.CaretIndex;
                    if (textBox.SelectionLength > 0)
                    {
                        caretIndex = textBox.SelectionStart;
                        text = text.Remove(textBox.SelectionStart, textBox.SelectionLength);
                    }

                    textBox.Text = text.Insert(caretIndex, pastedText);
                    textBox.CaretIndex = caretIndex + pastedText.Length;
                }
                e.CancelCommand();
            }
        }


        /// <summary>
        /// Handles the TargetUpdated event of the TextBox control, 
        /// Make sure Caret is set to end position when text is set from code behind
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataTransferEventArgs"/> instance containing the event data.</param>
        private static void TextBox_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (e.Source is TextBox textBox && e.Property.Name == "Text")
            {
                textBox.CaretIndex = textBox.Text.Length;
                textBox.ScrollToEnd();
            }
        }

    }
}
