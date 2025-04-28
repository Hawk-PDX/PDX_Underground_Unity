# PDX Underground Runtime Verification Checklist

This document provides a comprehensive checklist to ensure proper runtime functionality of PDX Underground game components. Use this checklist before playtesting or builds to verify that all systems will function correctly during gameplay.

## 1. BuzzUIController Runtime Checks

### UI Element References
- [ ] Verify `[SerializeField]` attributes are present for all UI elements
- [ ] Confirm Inspector assignments for all UI elements (buzzMeterFill, buzzStateIcon, etc.)
- [ ] Check that UI prefabs contain required components (Image, TextMeshProUGUI, etc.)
- [ ] Verify card prefab references are correctly assigned

### Null Reference Protection
- [ ] Confirm all UI element accesses have null checks (`if (buzzMeterFill != null)`)
- [ ] Verify all interface method calls check for null interfaces first
- [ ] Check array/list accesses with bounds validation before indexing
- [ ] Add fallback behavior for missing UI elements

### Error Logging & Debugging
- [ ] Add descriptive error messages for missing dependencies
- [ ] Include component name in debug logs for easier tracing
- [ ] Add warning logs for non-critical but unusual conditions
- [ ] Implement runtime visual indicators for missing references

### Coroutine Management
- [ ] Store all coroutine references to allow stopping (buzzMeterCoroutine, etc.)
- [ ] Add checks to stop existing coroutines before starting new ones
- [ ] Implement proper cleanup in OnDisable/OnDestroy methods
- [ ] Handle edge cases (zero duration, negative values, etc.)

## 2. Card System Runtime Safeguards

### Exception Handling
- [ ] Add try-catch blocks around card creation and modification operations
- [ ] Implement error handling for card loading/serialization failures
- [ ] Add recovery mechanisms for corrupted card data
- [ ] Check for invalid parameters (negative energy cost, etc.)

### Event Management
- [ ] Verify all event subscriptions have corresponding unsubscriptions in OnDestroy
- [ ] Check for null event handlers before invocation
- [ ] Add sender parameters to events for better context
- [ ] Consider weak event patterns for long-lived subscribers

### State Validation
- [ ] Add card state validation before performing actions
- [ ] Implement cooldown verification before card usage
- [ ] Add energy cost validation before card playing
- [ ] Check hand capacity before adding new cards

### Animation & Visual Effects
- [ ] Implement proper cleanup for interrupted card animations
- [ ] Add transition handling between card states
- [ ] Create fallback visuals for missing card art/assets
- [ ] Add graceful degradation for performance-intensive effects

## 3. Environment System Safety

### Scene Management
- [ ] Add error handling for scene loading failures
- [ ] Implement progress reporting during scene transitions
- [ ] Create fallback mechanisms for missing scenes
- [ ] Add timeout handling for stuck loading operations

### Environment Transitions
- [ ] Verify environment state transitions are handled correctly
- [ ] Add validation for environment type values
- [ ] Implement data consistency checks between environments
- [ ] Create smooth transitions between environment types

### Asset Management
- [ ] Add safeguards for missing environment assets
- [ ] Implement fallback textures/models for missing resources
- [ ] Add performance monitoring for heavy environment areas
- [ ] Consider dynamic loading/unloading for large environments

### Player Positioning
- [ ] Add boundary checks to prevent falling through world
- [ ] Implement reset positions for stuck players
- [ ] Add maximum velocity caps to prevent physics issues
- [ ] Create transition safeguards to ensure proper player placement

## 4. Unity-Specific Requirements

### Component Structure
- [ ] Verify MonoBehaviour inheritance for all runtime classes
- [ ] Add `[RequireComponent]` attributes where dependencies exist
- [ ] Check for correct component attachment in prefabs
- [ ] Verify serialized field types match component expectations

### Initialization Order
- [ ] Use Awake() for critical component setup
- [ ] Defer interface connections to Start() when appropriate
- [ ] Implement explicit initialization stages where needed
- [ ] Use coroutines for time-dependent initialization sequences

### Event System Integration
- [ ] Verify UI elements are connected to the EventSystem
- [ ] Check touch/mouse input handling for all interactive elements
- [ ] Add controller/keyboard navigation support
- [ ] Test input system under different control schemes

### Performance Considerations
- [ ] Add object pooling for frequently instantiated objects (cards, effects)
- [ ] Implement coroutine spreading for heavy operations
- [ ] Consider using Jobs system for computational-heavy tasks
- [ ] Add frame rate throttling for effects during critical gameplay

## 5. Testing Procedures

### Unit Tests
- [ ] Run existing unit tests before major changes
- [ ] Verify Card.cs tests pass
- [ ] Add/update tests for modified functionality
- [ ] Create new tests for edge cases discovered during development

### Integration Tests
- [ ] Test BuzzUIController with mock IBuzzSystem implementation
- [ ] Verify card system integration with UI components
- [ ] Test environment transitions end-to-end
- [ ] Verify cross-system communication (card effects on environment, etc.)

### Playtesting
- [ ] Test on low-end target hardware
- [ ] Verify controller/keyboard/touch input methods
- [ ] Test extended play sessions for memory leaks
- [ ] Validate game feel and responsiveness

## Final Verification

- [ ] Run static code analysis to catch common issues
- [ ] Perform a build and verify all systems function in built version
- [ ] Test startup/shutdown sequences
- [ ] Verify error reporting mechanisms function correctly

---

Last updated: April 11, 2025

