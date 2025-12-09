using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Amuse.UI.Effects
{
    public class DeeperColorEffect : ShaderEffect
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeeperColorEffect"/> class.
        /// </summary>
        public DeeperColorEffect()
        {
            PixelShader = new PixelShader();
            if (!DesignerProperties.GetIsInDesignMode(this))
                PixelShader.UriSource = new Uri("/Effects/DeeperColor.ps", UriKind.Relative);
            UpdateShaderValue(InputProperty);
        }

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(DeeperColorEffect), 0);

        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }
    }
}
