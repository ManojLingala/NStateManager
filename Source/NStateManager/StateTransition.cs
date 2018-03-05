﻿#region Copyright (c) 2018 Scott L. Carter
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is 
//distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and limitations under the License.
#endregion
using System;

namespace NStateManager
{
    internal class StateTransition<T, TState, TTrigger> : StateTransitionBase<T, TState, TTrigger>
    {
        public Func<T, bool> Condition { get; }

        public StateTransition(Func<T, TState> stateAccessor, Action<T, TState> stateMutator, TState toState, Func<T, bool> condition, string name, uint priority)
            : base(stateAccessor, stateMutator, toState, name, priority)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        public override StateTransitionResult<TState> Execute(ExecutionParameters<T, TTrigger> parameters, StateTransitionResult<TState> currentResult = null)
        {
            var startState = currentResult != null ? currentResult.StartingState : StateAccessor(parameters.Context);

            if (!Condition(parameters.Context))
            {
                if (currentResult != null)
                { return currentResult; }

                return new StateTransitionResult<TState>(startState
                , startState
                , startState
                , lastTransitionName: string.Empty
                , conditionMet: false);
            }

            StateMutator(parameters.Context, ToState);

            var transitionResult = currentResult == null 
                ? new StateTransitionResult<TState>(startState, startState, ToState, Name) 
                : new StateTransitionResult<TState>(startState, currentResult.CurrentState, ToState, Name);
            NotifyOfTransition(parameters.Context, transitionResult);

            return transitionResult;
        }
    }
}
