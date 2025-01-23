import os
import openai
import re
import logging
import sys
from datetime import datetime

# Set up logging
log_dir = os.path.join(os.getenv("GITHUB_WORKSPACE", "."), "logs")
os.makedirs(log_dir, exist_ok=True)
log_file = os.path.join(log_dir, f"test_generation_{datetime.now().strftime('%Y%m%d_%H%M%S')}.log")

logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler(log_file),
        logging.StreamHandler(sys.stdout)
    ]
)

# Set up OpenAI API key (assumed to be stored as an environment variable)
api_key = os.getenv("OPEN_AI_KEY")
if not api_key:
    logging.error("OPEN_AI_KEY environment variable not found")
    sys.exit(1)
openai.api_key = api_key

# Mapping of file extensions to programming languages
EXTENSION_LANGUAGE_MAP = {
    '.py': 'Python',
    '.js': 'JavaScript',
    '.ts': 'TypeScript',
    '.java': 'Java',
    '.cs': 'C#',
    '.cpp': 'C++',
    '.cxx': 'C++',
    '.cc': 'C++',
    '.hpp': 'C++',
    '.h': 'C++',
    '.rb': 'Ruby',
    '.go': 'Go',
    '.php': 'PHP',
    '.swift': 'Swift',
    '.kt': 'Kotlin',
    '.kts': 'Kotlin',
    '.cs': 'C#',
    '.csx': 'C#',
    '.vb': 'VB.NET',
    '.vbproj': 'VB.NET',
    '.vbhtml': 'VB.NET',
    '.vbcsproj': 'VB.NET',
    '.vbxml': 'VB.NET',
}

def get_code_files(repo_path):
    """Get code files from the review directory."""
    review_dir = os.path.join(repo_path, 'review')
    
    # Check if review directory exists
    if not os.path.exists(review_dir):
        logging.error("Review directory not found. Please create a 'review' directory and add files to be processed.")
        return []
    
    code_files = []
    for root, dirs, files in os.walk(review_dir):
        # Still exclude tests folder if it exists within review
        if 'tests' in dirs:
            dirs.remove('tests')
        if 'workflow_scripts' in dirs:
            dirs.remove('workflow_scripts')
            
        for file in files:
            ext = os.path.splitext(file)[1]
            if ext in EXTENSION_LANGUAGE_MAP:
                code_files.append(os.path.join(root, file))
    
    if not code_files:
        logging.error("No code files found in the review directory. Please add files to be processed.")
        return []
        
    return code_files

def read_file_content(file_path):
    """Read the content of a file."""
    with open(file_path, 'r', encoding='utf-8') as f:
        return f.read()

def get_language_from_extension(file_extension):
    """Get the programming language based on file extension."""
    return EXTENSION_LANGUAGE_MAP.get(file_extension, None)

def generate_unit_tests(file_content, language):
    """Generate unit test code using OpenAI's GPT model."""
    logging.info(f"Generating unit tests for {language} code")
    
    system_prompt = (
        f"You are an expert {language} developer specializing in unit test creation. "
        f"When given {language} code, you will generate comprehensive unit tests that: \n"
        f"1. Use the standard testing framework for {language} (e.g., unittest for Python, JUnit for Java)\n"
        f"2. Include both positive and negative test cases\n"
        f"3. Test edge cases and boundary conditions\n"
        f"4. Mock external dependencies appropriately\n"
        f"5. Follow testing best practices like arrange-act-assert pattern\n"
        f"6. Maintain high code coverage\n"
        f"7. Use descriptive test names that explain the scenario being tested\n\n"
        f"Output only the unit test code, without any explanations or markdown formatting."
    )

    user_prompt = f"""
                    Generate comprehensive unit test cases for the following {language} code: {language}

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
                    """
    try:
        logging.debug("Making API call to OpenAI")
        response = openai.ChatCompletion.create(
            model="gpt-4o", 
            messages=[
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_prompt}
            ],
            max_tokens=1500,
            temperature=0.5
        )
        logging.debug(f"OpenAI API response status: {response.choices[0].finish_reason}")
        
        assistant_response = response.choices[0].message.content
        logging.debug(f"Response length: {len(assistant_response)} characters")

        # Extract code between ``` and ```
        code_blocks = re.findall(r'```[^\n]*\n(.*?)```', assistant_response, re.DOTALL)

        if code_blocks:
            test_code = code_blocks[0]
            logging.info("Successfully extracted code block from response")
        else:
            # If no code blocks are found, assume the assistant returned only code
            test_code = assistant_response
            logging.info("No code blocks found, using entire response as test code")

        return test_code.strip()

    except openai.error.OpenAIError as e:
        logging.error(f"OpenAI API error: {str(e)}")
        raise
    except Exception as e:
        logging.error(f"Unexpected error in generate_unit_tests: {str(e)}")
        raise

def write_unit_test_file(test_file_path, test_code):
    """Write the generated unit tests to the specified test file."""
    with open(test_file_path, 'w', encoding='utf-8') as f:
        f.write(test_code)

def get_test_file_name(original_file_name, language):
    """Generate a test file name based on the language's conventions."""
    base_name, ext = os.path.splitext(original_file_name)
    if language == 'Python':
        test_file_name = f"test_{base_name}.py"
    elif language in ['JavaScript', 'TypeScript']:
        test_file_name = f"{base_name}.test{ext}"
    elif language == 'Java':
        test_file_name = f"{base_name}Test{ext}"
    elif language == 'C#':
        test_file_name = f"{base_name}Test{ext}"
    elif language == 'C++':
        test_file_name = f"{base_name}test{ext}"
    elif language == 'Ruby':
        test_file_name = f"test_{base_name}.rb"
    elif language == 'Go':
        test_file_name = f"{base_name}_test{ext}"
    elif language == 'PHP':
        test_file_name = f"{base_name}Test{ext}"
    elif language == 'Swift':
        test_file_name = f"{base_name}Tests{ext}"
    elif language == 'Kotlin':
        test_file_name = f"{base_name}Test{ext}"
    elif language == 'VB.NET':
        test_file_name = f"{base_name}Test{ext}"
    else:
        test_file_name = f"{base_name}_test{ext}"
    return test_file_name

def process_repository(repo_path):
    """Scan the repo, generate unit tests, and write them to test files."""
    logging.info(f"Starting repository processing at path: {repo_path}")
    
    # Create a tests folder in the root of the repository if it doesn't exist
    tests_folder = os.path.join(repo_path, 'tests')
    os.makedirs(tests_folder, exist_ok=True)
    logging.info(f"Created/verified tests folder at: {tests_folder}")

    # Get all code files in the repo (excluding the tests folder itself)
    code_files = get_code_files(repo_path)
    if not code_files:
        logging.info("No files to process. Workflow completed.")
        sys.exit(0)

    logging.info(f"Found {len(code_files)} code files to process")

    for index, code_file in enumerate(code_files, 1):
        logging.info(f"Processing file {index}/{len(code_files)}: {code_file}")
        try:
            file_content = read_file_content(code_file)

            # Skip empty files
            if not file_content.strip():
                logging.warning(f"Skipping empty file: {code_file}")
                continue

            # Determine the programming language
            _, ext = os.path.splitext(code_file)
            language = get_language_from_extension(ext)

            if not language:
                logging.warning(f"Could not determine language for file {code_file}. Skipping.")
                continue

            # Create a language-specific folder for tests
            language_folder = os.path.join(tests_folder, language)
            os.makedirs(language_folder, exist_ok=True)
            logging.debug(f"Created/verified language folder at: {language_folder}")

            # Create a test file name based on the original file name and language conventions
            original_file_name = os.path.basename(code_file)
            test_file_name = get_test_file_name(original_file_name, language)
            logging.debug(f"Generated test file name: {test_file_name}")

            # Create the test file path in the language folder
            test_file_path = os.path.join(language_folder, test_file_name)

            # Check if the test file already exists
            if os.path.exists(test_file_path):
                logging.info(f"Test file {test_file_path} already exists. Skipping.")
                continue

            # Generate unit tests using OpenAI
            try:
                test_code = generate_unit_tests(file_content, language)
                # Write the generated tests to a new file in the language folder
                write_unit_test_file(test_file_path, test_code)
                logging.info(f"Successfully generated and wrote tests for {code_file}")
            except Exception as e:
                logging.error(f"Failed to generate tests for {code_file}: {str(e)}")
                continue

        except Exception as e:
            logging.error(f"Error processing file {code_file}: {str(e)}")
            continue

    logging.info(f"Unit tests generation complete. Tests are stored in the 'tests' folder inside {repo_path}.")


if __name__ == "__main__":
    logging.info("Starting test generation script")
    try:
        # Automatically detect the repository path using GITHUB_WORKSPACE environment variable
        repo_path = os.getenv("GITHUB_WORKSPACE", ".")
        logging.info(f"Repository path: {repo_path}")
        
        if os.path.exists(repo_path):
            process_repository(repo_path)
        else:
            logging.error(f"Invalid repository path: {repo_path}")
            sys.exit(1)
    except Exception as e:
        logging.error(f"Fatal error: {str(e)}")
        sys.exit(1)
    logging.info("Test generation script completed")