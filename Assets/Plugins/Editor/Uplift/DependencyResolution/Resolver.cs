// --- BEGIN LICENSE BLOCK ---
/*
 * Copyright (c) 2019-present WeWantToKnow AS
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
// --- END LICENSE BLOCK ---


using Uplift.Common;
using Uplift.Packages;
using Uplift.Schemas;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Uplift.DependencyResolution
{
	class Resolver : IDependencySolver
	{
		public PackageList packageList;
		DependencyGraph baseGraph = new DependencyGraph();
		LinkedList<DependencyDefinition> originalDependencies;
		Stack<State> stateStack = new Stack<State>();

		public Resolver(DependencyGraph baseGraph, PackageList packageList)
		{

			this.baseGraph = baseGraph;
			this.packageList = packageList;
			if (packageList.GetType() == typeof(PackageListStub))
			{
				packageList.SetPackages(((PackageListStub)packageList).GetAllPackageRepo());
			}
		}

		public Resolver(PackageList packageList)
		{
			this.packageList = packageList;
		}

		public void pushInitialState(DependencyDefinition[] dependencies, DependencyGraph dg)
		{
			Debug.Log("Pushing initial state");

			this.originalDependencies = new LinkedList<DependencyDefinition>(dependencies);

			//Create nodes for original dependencies
			foreach (DependencyDefinition requested in originalDependencies)
			{
				DependencyNode node;
				if (!dg.Contains(requested.Name))
				{
					node = new DependencyNode(requested);
					dg.AddNode(node);
				}
				else
				{
					node = dg.FindByName(requested.Name);
				}

				if (node.restrictions.ContainsKey("initial"))
				{
					throw new IncompatibleRequirementException("initial requirements cannot have twice the same requirement");
				}
				else
				{
					node.restrictions["initial"] = requested.Requirement;
				}
			}

			//Create dependency state for original dependencies
			LinkedList<DependencyDefinition> currentDependencies = originalDependencies;
			List<Conflict> conflicts = new List<Conflict>();
			DependencyState initialState = new DependencyState(currentDependencies,
																dg,
																new List<PossibilitySet>(), //possibilities
																0,
																conflicts, //conflicts
																new Dictionary<string, List<IVersionRequirement>>() //requirements history
															);
			stateStack.Push(initialState);
			Debug.Log("===> Initial state : ");
			Debug.Log(initialState);
		}

		// Returns list of possibility sets according to the requirements of a given state
		// Each possibility set representing a group of versions for a dependency which 
		// share the same sub-dependency requirements and are contiguous.
		List<PossibilitySet> GeneratePossibilitySets(State state, PackageList packageList)
		{
			List<PossibilitySet> possibilities = state.possibilities;
			foreach (DependencyDefinition dependency in state.requirements)
			{
				if (!possibilities.Exists(possibilitySet => possibilitySet.name == dependency.Name))
				{
					Debug.Log(dependency.Name + " is not listed in possibility sets");
					possibilities.AddRange(PossibilitySet.GetPossibilitySetsForGivenPackage(dependency.Name, packageList));
				}
			}
			return possibilities;
		}

		// A debug function to display the state stack.
		void ShowStateStack()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("==== State Stack ====");
			foreach (State state in stateStack)
			{
				sb.AppendLine("[ (" + state.depth.ToString() + ") " + state.GetType().ToString() + " ]");
			}
			sb.AppendLine("================");
			Debug.Log(sb.ToString());
		}

		// A debug function to display possibility sets.
		void ShowPossibilitySets(List<PossibilitySet> possibilitySets)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Possibility Sets for current state : ");
			foreach (PossibilitySet possibilitySet in possibilitySets)
			{
				sb.AppendLine("* " + possibilitySet.ToString());
			}
			Debug.Log(sb.ToString());
		}

		// A debug function to display packages in resolution
		void ShowResolution(List<PackageRepo> resolution)
		{
			StringBuilder sb = new StringBuilder();

			foreach (PackageRepo pkg in resolution)
			{
				sb.AppendLine(pkg.Package.PackageName + " : " + pkg.Package.PackageVersion);
			}
			Debug.Log(sb.ToString());
		}

		public List<PackageRepo> SolveDependencies(DependencyDefinition[] dependencies)
		{
			return SolveDependencies(dependencies, null);
		}

		//Main function which process the different states in the state stack to solve dependencies
		public List<PackageRepo> SolveDependencies(DependencyDefinition[] dependencies, PackageRepo[] startingPackages)
		{
			Debug.Log("Solve dependencies");

			DependencyGraph dg = new DependencyGraph(startingPackages);
			pushInitialState(dependencies, dg);

			// Final results
			List<PackageRepo> resolution = new List<PackageRepo>();

			//FIXME change this for final version
			int i = 1000;
			while (i > 0)//stateStack.Count > 0)
			{
				i--;
				if (stateStack.Count == 0)
				{
					break;
				}

				ShowStateStack();

				// Process Topmost state in stack
				State currentState = stateStack.Peek();

				// Process either a dependency State or a Possibility State
				if (currentState.GetType() == typeof(DependencyState))
				{
					Debug.Log("Current state is dependency state !");
					List<PossibilitySet> possibilitySets = GeneratePossibilitySets(currentState, packageList);
					currentState.possibilities = possibilitySets;

					ShowPossibilitySets(possibilitySets);

					// Check if resolution is over
					if (currentState.requirements.Count == 0)
					{
						// No more requirement to fulfill resolution is over
						Debug.Log("No more requirements to match, getting solutions : ");
						Debug.Log(currentState.activated.ToString());
						resolution = ((DependencyState)currentState).GetResolution();
						break;
					}
					else
					{
						// Resolution is not over, generating new possibility state for a remaining dependency
						PossibilityState newPossibilityState = ((DependencyState)currentState).PopPossibilityState();
						if (newPossibilityState != null)
						{
							Debug.Log("Add new possibility state in stack");
							stateStack.Push(newPossibilityState);
						}
					}
				}
				else if (currentState.GetType() == typeof(PossibilityState))
				{
					Debug.Log("Current state is possibility state !");
					// Find resolution for current possibility state
					DependencyState newState = ((PossibilityState)currentState).SolveState();

					// Check for conflicts
					if (currentState.conflicts != null && currentState.conflicts.Count > 0)
					{
						// Conflicts were found, rewind in a previous state to find solution
						Conflict conflict = currentState.conflicts.ToArray()[0];
						Rewinder rewinder = new Rewinder(stateStack);
						stateStack = rewinder.UnwindForConflict(conflict, packageList);
						currentState.conflicts.Remove(conflict);
					}
					else
					{
						// State Resolution is successfull
						Debug.Log("Push new dependency state in stack");
						if (newState == null)
						{
							Debug.LogError("error, no viable solution found");
							break;
						}
						stateStack.Push(newState);
					}
				}
				else
				{
					Debug.LogError("Error : Current state is neither possibility or dependency state");
				}
			}

			Debug.Log("===== Final resolution : =====");
			ShowResolution(resolution);
			return resolution;
		}
	}
}