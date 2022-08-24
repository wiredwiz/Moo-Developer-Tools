#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="ProducerConsumer.cs">
// Copyright (c)  2022
// </copyright>
//
// This is a modified version of Jon Skeet's old ProducerConsumer.
// He deserves most of the credit
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

using System.Collections;

namespace Org.Edgerunner.Messaging;

public class ProducerConsumer<T>
{
   protected readonly object ListLock = new();

   protected readonly Queue<IMessageToken<T>> Queue = new();

   public void Produce(IMessageToken<T> token)
   {
      lock (ListLock)
      {
         Queue.Enqueue(token);

         // We always need to pulse, even if the queue wasn't
         // empty before. Otherwise, if we add several items
         // in quick succession, we may only pulse once, waking
         // a single thread up, even if there are multiple threads
         // waiting for items.
         Monitor.Pulse(ListLock);
      }
   }

   public IMessageToken<T> Consume()
   {
      lock (ListLock)
      {
         // If the queue is empty, wait for an item to be added
         // Note that this is a while loop, as we may be pulsed
         // but not wake up before another thread has come in and
         // consumed the newly added object. In that case, we'll
         // have to wait for another pulse.
         while (Queue.Count==0)
         {
            // This releases listLock, only reacquiring it
            // after being woken up by a call to Pulse
            Monitor.Wait(ListLock);
         }
         return Queue.Dequeue();
      }
   }
}