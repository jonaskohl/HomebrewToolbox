using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WiiBrewToolbox
{
    public class SkinControlInformation
    {
        private List<SkinControlImageDescription> imageDescriptions = new List<SkinControlImageDescription>();
        private List<SkinControlColorDescription> colorDescriptions = new List<SkinControlColorDescription>();

        public ReadOnlyCollection<SkinControlImageDescription> ImageDescriptions
        {
            get => imageDescriptions.AsReadOnly();
        }

        public ReadOnlyCollection<SkinControlColorDescription> ColorDescriptions
        {
            get => colorDescriptions.AsReadOnly();
        }

        public static SkinControlInformation Default = new SkinControlInformation()
        {
            imageDescriptions = new List<SkinControlImageDescription>(new[] { SkinControlImageDescription.ButtonsDefault }),
            colorDescriptions = new List<SkinControlColorDescription>(new[] { SkinControlColorDescription.ButtonsDefault })
        };

        public static SkinControlInformation FromXML(XDocument doc)
        {
            var instance = new SkinControlInformation();

            Debug.Assert(doc.Root.Name == "controlDef");
            Debug.Assert(doc.Root.Attribute("version")?.Value == "1");

            instance.imageDescriptions = doc.Root.Elements("image").Select(xelem =>
            {
                var name = xelem.Attribute("name").Value;
                var states = xelem.Elements("state").Select(xstate =>
                {
                    var state = GetState(xstate.Attribute("for").Value);
                    var from = GetPoint(xstate.Attribute("from").Value);
                    var size = new Size(GetPoint(xstate.Attribute("size").Value));
                    var slices = GetPadding(xstate.Attribute("slices").Value);
                    var padding = GetPadding(xstate.Attribute("padding")?.Value);

                    return new SkinControlImageState()
                    {
                        ForState = state,
                        From = from,
                        Size = size,
                        Slices = slices,
                        Padding = padding
                    };
                });

                return new SkinControlImageDescription(name, states);
            }).ToList();

            instance.colorDescriptions = doc.Root.Elements("colors").Select(xelem =>
            {
                var name = xelem.Attribute("name").Value;
                var states = xelem.Elements("color").Select(xstate =>
                {
                    var state = GetState(xstate.Attribute("for").Value);
                    var key = xstate.Attribute("key").Value;
                    var color = GetColor(xstate.Value);

                    return new SkinControlColorState()
                    {
                        ForState = state,
                        Key = key,
                        color = color
                    };
                });

                return new SkinControlColorDescription(name, states);
            }).ToList();

            return instance;
        }

        private static Color GetColor(string value)
        {
            return ColorTranslator.FromHtml(value);
        }

        private static Padding GetPadding(string value)
        {
            if (value == null)
                return Padding.Empty;

            var fields = value.Split(new[] { ',' }, 4, StringSplitOptions.RemoveEmptyEntries);
            var numbers = fields.Select(n => int.Parse(n, NumberStyles.Integer, CultureInfo.InvariantCulture)).ToArray();
            return new Padding(numbers[0], numbers[1], numbers[2], numbers[3]);
        }

        private static Point GetPoint(string value)
        {
            var fields = value.Split(new[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);
            var x = int.Parse(fields[0], NumberStyles.Integer, CultureInfo.InvariantCulture);
            var y = int.Parse(fields[1], NumberStyles.Integer, CultureInfo.InvariantCulture);
            return new Point(x, y);
        }

        private static ButtonImageState GetState(string stateStr)
        {
            switch (stateStr)
            {
                case "normal": return ButtonImageState.Normal;
                case "hot": return ButtonImageState.Hot;
                case "pressed": return ButtonImageState.Pressed;
                case "disabled": return ButtonImageState.Disabled;
            }

            throw new Exception("Invalid state");
        }
    }
}
