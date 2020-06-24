﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using KoiX;
using KoiX.Geometry;
using KoiX.Input;
using KoiX.Text;

using Boku.Common;


namespace KoiX.UI
{
    /// <summary>
    /// Acts just like a radio button except is represented by a graphic image (texture).
    /// </summary>
    public class GraphicRadioButton : BaseWidget
    {
        #region Members

        Texture2D texture;
        string textureName;
        Vector2 textureDisplaySize;

        string labelId;
        string labelText;

        UIState prevCombinedState = UIState.Inactive;

        List<GraphicRadioButton> siblings;      // List of all radio buttons in this set.  Used for clearing
                                                // selection of others when this one is set.

        // TODO (scoy) Do we need a seperate theme for GraphicRadioButton?
        RadioButtonTheme curTheme;              // Colors and sizes for current state.

        Twitchable<Color> bodyColor;
        Twitchable<Color> outlineColor;
        Twitchable<float> outlineWidth;
        Twitchable<float> cornerRadius;

        #endregion

        #region Accessors

        public new bool Selected
        {
            get { return base.Selected; }
            set
            {
                if (base.Selected != value)
                {
                    base.Selected = value;

                    // If we're setting this GraphicRadioButton "on" then
                    // all its siblings need to be "off".
                    if (value == true)
                    {
                        foreach (GraphicRadioButton rb in siblings)
                        {
                            if (rb != this)
                            {
                                rb.Selected = false;
                            }
                        }
                    }

                    // TODO (scoy) Should we only call OnChange when selected or
                    // should be call any time the state changes?
                    if (Selected)
                    {
                        SetFocus(overrideInactive: true);
                        OnChange();
                    }
                }   // end if value changes.

            }
        }

        public Color BodyColor
        {
            get { return bodyColor.Value; }
            set { bodyColor.Value = value; }
        }

        public Color _BodyColor
        {
            get { return bodyColor.TargetValue; }
            set { bodyColor.TargetValue = value; }
        }

        public Color OutlineColor
        {
            get { return outlineColor.Value; }
            set { outlineColor.Value = value; }
        }

        public Color _OutlineColor
        {
            get { return outlineColor.TargetValue; }
            set { outlineColor.TargetValue = value; }
        }

        public float OutlineWidth
        {
            get { return outlineWidth.Value; }
            set { outlineWidth.Value = value; }
        }

        public float _OutlineWidth
        {
            get { return outlineWidth.TargetValue; }
            set { outlineWidth.TargetValue = value; }
        }

        public float CornerRadius
        {
            get { return cornerRadius.Value; }
            set { cornerRadius.Value = value; }
        }

        public float _CornerRadius
        {
            get { return cornerRadius.TargetValue; }
            set { cornerRadius.TargetValue = value; }
        }

        public string TextureName
        {
            get { return textureName; }
        }

        public string LabelId
        {
            get { return labelId; }
            set
            {
                if (labelId != value)
                {
                    labelId = value;
                    labelText = Strings.Localize(labelId);
                }
            }
        }

        /// <summary>
        /// Optional label string.  This is not displayed but can
        /// be retrieved as needed.
        /// </summary>
        public string LabelText
        {
            get { return labelText; }
            set { labelText = value; labelId = null; }
        }

        #endregion

        #region Public

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentDialog"></param>
        /// <param name="siblings"></param>
        /// <param name="textureDisplaySize"></param>
        /// <param name="textureName">Resource name for texture.</param>
        /// <param name="texture">Texture ref.  Useful for dynamically created textures.</param>
        /// <param name="labelId">Optional label info.  Is not displayed but can be retrived as needed.</param>
        /// <param name="labelText"></param>
        /// <param name="OnChange">Callback which is called when this button is selected.  Note if button is already selected then callback is still called.</param>
        /// <param name="theme"></param>
        /// <param name="data"></param>
        public GraphicRadioButton(BaseDialog parentDialog, List<GraphicRadioButton> siblings, Vector2 textureDisplaySize, string textureName = null, Texture2D texture = null, string labelId = null, string labelText = null, Callback OnChange = null, ThemeSet theme = null, string id = null, object data = null)
            : base(parentDialog, OnChange: OnChange, theme: theme, id: id, data: data)
        {
            this.siblings = siblings;
            this.texture = texture;
            this.textureName = textureName;
            this.textureDisplaySize = textureDisplaySize;

            this.labelId = labelId;
            this.labelText = labelText;
            if (labelId != null)
            {
                this.labelText = Strings.Localize(labelId);
            }

            // Add self.
            siblings.Add(this);

            if (theme == null)
            {
                theme = Theme.CurrentThemeSet;
            }

            // Default to Inactive state.
            prevCombinedState = UIState.Inactive;
            curTheme = theme.RadioButtonNormal.Clone() as RadioButtonTheme;

            // Create all the Twitchables and set initial values.
            bodyColor = new Twitchable<Color>(Theme.TwitchTime, TwitchCurve.Shape.EaseInOut, parent: this, startingValue: curTheme.BodyColor);
            outlineColor = new Twitchable<Color>(Theme.TwitchTime, TwitchCurve.Shape.EaseInOut, parent: this, startingValue: curTheme.OutlineColor);
            outlineWidth = new Twitchable<float>(Theme.TwitchTime, TwitchCurve.Shape.EaseInOut, parent: this, startingValue: curTheme.OutlineWidth);
            cornerRadius = new Twitchable<float>(Theme.TwitchTime, TwitchCurve.Shape.EaseInOut, parent: this, startingValue: 4.0f);

            localRect.Size = textureDisplaySize;
        }   // end of c'tor

        public override void Update(SpriteCamera camera, Vector2 parentPosition)
        {
            // Needed to handle focus changes.
            base.Update(camera, parentPosition);

            if (Hover)
            {
                SetFocus();
            }

            UIState combinedState = CombinedState;
            if (combinedState != prevCombinedState)
            {
                // Set new state params.  Note that dirty flag gets
                // set internally by setting individual values so
                // we don't need to worry about it here.
                switch (combinedState)
                {
                    case UIState.Disabled:
                    case UIState.DisabledSelected:
                        curTheme = theme.RadioButtonDisabled;
                        break;

                    case UIState.Active:
                    case UIState.ActiveHover:
                        curTheme = theme.RadioButtonNormal;
                        break;

                    case UIState.ActiveFocused:
                    case UIState.ActiveFocusedHover:
                        curTheme = theme.RadioButtonNormalFocused;
                        break;

                    case UIState.ActiveSelected:
                    case UIState.ActiveSelectedHover:
                        curTheme = theme.RadioButtonSelected;
                        break;

                    // For GraphicRadioButton selected == checked.
                    case UIState.ActiveSelectedFocused:
                    case UIState.ActiveSelectedFocusedHover:
                        curTheme = theme.RadioButtonSelectedFocused;
                        break;

                    default:
                        // Should only happen on state.None
                        break;

                }   // end of switch

                prevCombinedState = combinedState;

            }   // end if state changed.

        }   // end of Update()

        public override void Render(SpriteCamera camera, Vector2 parentPosition)
        {
            Vector2 pos = Position + parentPosition;
            Vector2 center = pos + new Vector2(textureDisplaySize.X / 2.0f, textureDisplaySize.Y - curTheme.CornerRadius);

            if (InFocus)
            {
                // Focus color highlight under full object.
                RectangleF rect = LocalRect;
                rect.Position = pos;
                rect.Inflate(8.0f);

                RoundedRect.Render(camera, rect, 8.0f, theme.FocusColor);
            }

            // Render texture.
            SpriteBatch batch = KoiLibrary.SpriteBatch;
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.ViewMatrix);
            {
                Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, (int)textureDisplaySize.X, (int)textureDisplaySize.Y);
                batch.Draw(texture, rect, Color.White);
            }
            batch.End();

            // Disc should be centered at bottom of texture.
            Disc.Render(camera, center, curTheme.CornerRadius, curTheme.BodyColor,
                    outlineWidth: curTheme.OutlineWidth, outlineColor: curTheme.OutlineColor,
                    bevelStyle: BevelStyle.Round, bevelWidth: curTheme.CornerRadius);

            // Needed for debug rendering.
            base.Render(camera, parentPosition);

        }   // end of Render()

        public override void RegisterForInputEvents()
        {
            // Register to get left down mouse event.  
            KoiLibrary.InputEventManager.RegisterForEvent(this, InputEventManager.Event.MouseLeftDown);

            // Also register for Keyboard.  If this button has focus and enter is pressed that's the same as a mouse click.
            KoiLibrary.InputEventManager.RegisterForEvent(this, InputEventManager.Event.Keyboard);

            // Tap also toggles checked.
            KoiLibrary.InputEventManager.RegisterForEvent(this, InputEventManager.Event.Tap);

            // If we have focus, gamepad A should toggle state.
            KoiLibrary.InputEventManager.RegisterForEvent(this, InputEventManager.Event.GamePad);

        }   // end of RegisterForInputEvents()

        #endregion

        #region InputEventHandler

        public override bool ProcessMouseLeftDownEvent(MouseInput input)
        {
            Debug.Assert(Active);

            if (KoiLibrary.InputEventManager.MouseFocusObject == null)
            {
                if (KoiLibrary.InputEventManager.MouseHitObject == this)
                {
                    // Claim mouse focus as ours.
                    KoiLibrary.InputEventManager.MouseFocusObject = this;

                    // Register to get left up events.
                    KoiLibrary.InputEventManager.RegisterForEvent(this, InputEventManager.Event.MouseLeftUp);

                    return true;
                }
            }

            return false;
        }   // end of ProcessMouseLeftDownEvent()

        public override bool ProcessMouseLeftUpEvent(MouseInput input)
        {
            Debug.Assert(Active);

            if (KoiLibrary.InputEventManager.MouseFocusObject == this)
            {
                // Release mouse focus.
                if (KoiLibrary.InputEventManager.MouseFocusObject == this)
                {
                    KoiLibrary.InputEventManager.MouseFocusObject = null;
                }

                // Stop getting move and up events.
                KoiLibrary.InputEventManager.UnregisterForEvent(this, InputEventManager.Event.MouseMove);
                KoiLibrary.InputEventManager.UnregisterForEvent(this, InputEventManager.Event.MouseLeftUp);

                // If mouse up happens over box, fine.  If not, ignore.
                if (KoiLibrary.InputEventManager.MouseHitObject == this)
                {
                    // Set to "on".
                    Selected = true;
                }

                return true;
            }
            return false;
        }   // end of ProcessMouseLeftUpEvent()

        public override bool ProcessKeyboardEvent(KeyInput input)
        {
            Debug.Assert(Active);

            if (InFocus && input.Key == Microsoft.Xna.Framework.Input.Keys.Enter && !input.Modifier)
            {
                // If inFocus, toggle state.
                if (InFocus)
                {
                    // Set to "on".
                    Selected = true;

                    return true;
                }
            }

            return base.ProcessKeyboardEvent(input);
        }   // end of ProcessKeyboardEvent()

        public override bool ProcessTouchTapEvent(TapGestureEventArgs gesture)
        {
            Debug.Assert(Active);

            // Did this gesture hit us?
            if (gesture.HitObject == this)
            {
                // Set to "on".
                Selected = true;

                return true;
            }

            return base.ProcessTouchTapEvent(gesture);
        }   // end of ProcessTouchTapEvent()

        public override bool ProcessGamePadEvent(GamePadInput pad)
        {
            Debug.Assert(Active);

            if (InFocus)
            {
                if (pad.ButtonA.WasPressed && InFocus)
                {
                    // Set to "on".
                    Selected = true;

                    return true;
                }
            }

            return base.ProcessGamePadEvent(pad);
        }
        #endregion

        #region Internal

        public override void LoadContent()
        {
            if (!string.IsNullOrEmpty(textureName) && DeviceResetX.NeedsLoad(texture))
            {
                texture = KoiLibrary.LoadTexture2D(textureName);
            }

            base.LoadContent();
        }   // end of LoadContent()

        public override void UnloadContent()
        {
            DeviceResetX.Release(ref texture);

            base.UnloadContent();
        }   // end of UnloadContent()

        #endregion

    }   // end of class GraphicRadioButton

}   // end of namespace KoiX.UI
