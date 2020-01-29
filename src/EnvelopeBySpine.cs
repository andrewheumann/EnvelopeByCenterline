using System.Collections.Generic;
using System.Linq;
using System;
using Elements;
using Elements.Geometry;
using GeometryEx;
using ClipperLib;

namespace EnvelopeBySpine
{
    public static class EnvelopeBySpine
    {
        /// <summary>
        /// The EnvelopeBySpine function.
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <param name="input">The arguments to the execution.</param>
        /// <returns>A EnvelopeBySpineOutputs instance containing computed results and the model with any new elements.</returns>
        public static EnvelopeBySpineOutputs Execute(Dictionary<string, Model> inputModels, EnvelopeBySpineInputs input)
        {

            var spine = input.Spine;
            var perimeter = spine.Offset(input.BarWidth / 2, EndType.Butt).First();

            // Create the foundation Envelope.
            var extrude = new Elements.Geometry.Solids.Extrude(perimeter, input.FoundationDepth, Vector3.ZAxis, 0.0, false);
            var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>() { extrude });
            var fndMatl = new Material("foundation", new Color(0.60000002384185791, 0.60000002384185791, 0.60000002384185791, 0.60000002384185791), 0.0f, 0.0f);
            var envMatl = new Material("envelope", new Color(0.30000001192092896, 0.699999988079071, 0.699999988079071, 0.60000002384185791), 0.0f, 0.0f);
            var envelopes = new List<Envelope>()
            {
                new Envelope(perimeter, input.FoundationDepth * -1, input.FoundationDepth, Vector3.ZAxis,
                             0.0, new Transform(0.0, 0.0, input.FoundationDepth * -1), fndMatl, geomRep, Guid.NewGuid(), "")
            };

            // Create the Envelope at the location's zero plane.

            extrude = new Elements.Geometry.Solids.Extrude(perimeter, input.BuildingHeight, Vector3.ZAxis, 0.0, false);
            geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>() { extrude });
            envelopes.Add(new Envelope(perimeter, 0.0, input.BuildingHeight, Vector3.ZAxis, 0.0,
                          new Transform(), envMatl, geomRep, Guid.NewGuid(), ""));

            var output = new EnvelopeBySpineOutputs(input.BuildingHeight, input.FoundationDepth);
            envelopes = envelopes.OrderBy(e => e.Elevation).ToList();
            foreach (var env in envelopes)
            {
                output.model.AddElement(env);
            }
            return output;
        }

    }
}