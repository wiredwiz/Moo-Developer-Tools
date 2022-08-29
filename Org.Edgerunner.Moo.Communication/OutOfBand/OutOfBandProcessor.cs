#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="OutOfBandProcessor.cs">
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

using Org.Edgerunner.Messaging;
using Org.Edgerunner.Moo.Communication.Interfaces;

namespace Org.Edgerunner.Moo.Communication.OutOfBand;

/// <summary>
/// A class that represents a processor for client out of band messages.
/// </summary>
public class OutOfBandProcessor : IOutOfBandWorkerQueue, IOutOfBandProcessor
{
   /// <summary>
   /// Initializes a new instance of the <see cref="OutOfBandProcessor"/> class.
   /// </summary>
   /// <param name="clientSession">The moo client session.</param>
   public OutOfBandProcessor(IMooClientSession clientSession)
   {
      ClientSession = clientSession;
      IncomingQueue = new ProducerConsumer<string>();
      _TokenSource = CancellationTokenSource.CreateLinkedTokenSource();
      _Queue = new Queue<string>();
   }

   private Task? _ProcessorTask;

   private readonly CancellationTokenSource _TokenSource;

   private readonly Queue<string> _Queue;

   protected IMooClientSession ClientSession { get; }

   public ProducerConsumer<string> IncomingQueue { get; }

   /// <summary>
   /// Starts the out of band processing.
   /// </summary>
   public void Start()
   {
      if (_ProcessorTask == null || _ProcessorTask.IsCanceled || _ProcessorTask.IsCompleted)
         _ProcessorTask = ProcessCommands(_TokenSource);
   }

   /// <summary>
   /// Stops the processing.
   /// </summary>
   public void Stop()
   {
      _TokenSource.Cancel();
      _ProcessorTask?.Wait();
      _ProcessorTask = null;
   }

   protected Task ProcessCommands(CancellationTokenSource tokenSource)
   {
      return Task.Run(
                      () =>
                      {
                         while (true)
                         {
                            var command = IncomingQueue.Consume();
                            if (tokenSource.Token.IsCancellationRequested)
                               break;
                         }
                      });
   }

   public void Push(string command)
   {
      _Queue.Enqueue(command);
   }

   public string Pop()
   {
      return _Queue.Dequeue();
   }
}