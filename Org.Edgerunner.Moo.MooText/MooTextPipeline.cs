#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="MooTextPipeline.cs">
// Copyright (c)  2022
// </copyright>
//
// BSD 3-Clause License
//
// Copyright (c) 2022,
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

using System.Text;

namespace Org.Edgerunner.Moo.MooText;

public abstract class MooTextPipeline
{
   /// <summary>
   /// Initializes a new instance of the <see cref="MooTextPipeline"/> class.
   /// </summary>
   protected MooTextPipeline()
   {
   }

   /// <summary>
   /// Gets or sets the next pipe in the pipeline.
   /// </summary>
   /// <value>
   /// The next pipe.
   /// </value>
   internal MooTextPipeline? NextPipe { get; set; }

   /// <summary>
   /// Processes the specified text.
   /// </summary>
   /// <param name="text">The text.</param>
   /// <param name="position">The current position.</param>
   /// <param name="output">The output <see cref="StringBuilder"/>.</param>
   /// <returns><c>true</c> if this pipeline instance or another pipe farther down processed the current position; <c>false</c> otherwise.</returns>
   internal virtual bool Process(ref char[] text, ref int position, ref StringBuilder output)
   {
      if (NextPipe != null)
         return NextPipe.Process(ref text, ref position, ref output);

      return false;
   }

   /// <summary>
   /// Performs pre-processing changes.
   /// </summary>
   /// <param name="output">The output <see cref="StringBuilder"/>.</param>
   internal virtual void PreProcessing(ref StringBuilder output)
   {
      if (NextPipe != null)
         NextPipe.PreProcessing(ref output);
   }

   /// <summary>
   /// Performs post processing changes.
   /// </summary>
   /// <param name="output">The output <see cref="StringBuilder"/>.</param>
   internal virtual void PostProcessing(ref StringBuilder output)
   {
      if (NextPipe != null)
         NextPipe.PostProcessing(ref output);
   }

   /// <summary>
   /// Adds the supplied pipeline into our pipeline linkage.
   /// </summary>
   /// <param name="pipeline">The pipeline.</param>
   internal virtual void AddPipeline(MooTextPipeline pipeline)
   {
      if (NextPipe == null)
      {
         NextPipe = pipeline;
         return;
      }

      var current = NextPipe;
      while (current.NextPipe != null)
         current = current.NextPipe;

      current.NextPipe = pipeline;
   }

   /// <summary>
   /// Resets this instance and the linkage from this pipeline.
   /// </summary>
   internal virtual void Reset()
   {
      NextPipe?.Reset();
   }
}