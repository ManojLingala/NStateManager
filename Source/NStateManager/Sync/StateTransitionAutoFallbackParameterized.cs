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

namespace NStateManager.Sync
{
    internal class StateTransitionAutoFallbackParameterized<T, TState, TTrigger, TParam> : StateTransitionParameterized<T, TState, TTrigger, TParam>
        where TState : IComparable
        where TParam : class
    {
        private readonly IStateMachine<T, TState, TTrigger> _stateMachine;
        private readonly TState _startState;
        private readonly TState _triggerState;

        public StateTransitionAutoFallbackParameterized(IStateMachine<T, TState, TTrigger> stateMachine
          , TState startState
          , TState triggerState
          , TState toState
          , Func<T, TParam, bool> condition
          , string name
          , uint priority)
            : base(stateMachine.StateAccessor, stateMachine.StateMutator, toState, condition, name, priority)
        {
            _stateMachine = stateMachine;
            _startState = startState;
            _triggerState = triggerState;
        }

        public override StateTransitionResult<TState, TTrigger> Execute(ExecutionParameters<T, TTrigger> parameters
          , StateTransitionResult<TState, TTrigger> currentResult = null)
        {
            if (currentResult != null
                && _startState.IsEqual(currentResult.PreviousState)
                && (_triggerState.IsEqual(currentResult.CurrentState)
                    || _stateMachine.IsInState(parameters.Context, _triggerState)))
            { return base.Execute(parameters, currentResult); }

            return GetFreshResult(parameters
              , currentResult
              , StateAccessor(parameters.Context)
              , wasCancelled: false
              , transitionDefined: true
              , conditionMet: false);
        }
    }
}