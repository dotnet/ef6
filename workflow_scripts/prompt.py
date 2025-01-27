CODE_REVIEW_PROMPT = '''
You are a senior C#/.NET software engineer. Review the following code change in `{file_name}`, focusing on maintainability, performance, and security. Format each comment using this template:

[Line X] Issue Title
- Current: <problematic code>
- Suggested: <improved code>
- Why: Brief explanation focused on C#/.NET best practices, performance, or security

Example issue:

[Line 42] Potential NullReferenceException
- Current: var result = user.Profile.Name.ToUpper();
- Suggested: var result = user?.Profile?.Name?.ToUpper() ?? string.Empty;
- Why: Use null conditional operators to safely handle potential null values in property chains

Code to review:
```diff
{patch}
```

Relevant context:
```diff
{relevant_context}
```

Guidelines:
1. Focus on high-impact issues only
2. Be direct and specific - no introductory text
3. Check for:
   - NullReferenceException risks
   - Exception handling (avoid empty catches)
   - Resource disposal (IDisposable)
   - Performance (LINQ in loops, etc)
   - Thread safety (static/shared state)
   - Security vulnerabilities (e.g., SQL injection, XSS)
   - Code readability and maintainability
   - Performance (LINQ in loops, etc)
4. Reference context code if relevant
5. Keep explanations under 2 sentences
'''

TEST_GENERATION_PROMPT = '''Generate comprehensive unit test cases for the following {language} code: {language}

{file_content}

Please follow these guidelines when generating tests:

1. Test Coverage:
   - Test all public functions and methods
   - Include both positive and negative test cases
   - Test edge cases and boundary conditions
   - Verify error handling and exceptions

2. Test Structure:
   - Group related tests into test classes/suites
   - Use descriptive test names that explain the scenario
   - Follow the Arrange-Act-Assert pattern
   - Keep tests focused and atomic

3. Best Practices:
   - Mock external dependencies appropriately
   - Avoid test interdependencies
   - Use setup/teardown methods when needed
   - Follow DRY principles while maintaining test clarity

4. Documentation:
   - Add clear docstrings/comments explaining test purpose
   - Document test assumptions and prerequisites
   - Explain complex test scenarios
   - Note any specific test data requirements

5. Code Quality:
   - Write clean, maintainable test code
   - Use meaningful variable names
   - Follow language-specific testing conventions
   - Include assertions with descriptive messages

Generate tests that would be suitable for a production codebase.
'''