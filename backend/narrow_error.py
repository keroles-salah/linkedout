import re, subprocess, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

with open(r'C:\Users\ke_ro\.openclaw-autoclaw\workspace\linkdout\backend\temp_js.js', 'r', encoding='utf-8') as f:
    js = f.read()

lines = js.split('\n')

# Binary search for the error
def check_syntax(code, label):
    with open(r'C:\Users\ke_ro\.openclaw-autoclaw\workspace\linkdout\backend\_test.js', 'w', encoding='utf-8') as f:
        f.write(code)
    result = subprocess.run(['node', '--check', r'C:\Users\ke_ro\.openclaw-autoclaw\workspace\linkdout\backend\_test.js'], 
                          capture_output=True, text=True)
    if result.returncode != 0:
        print(f"FAIL {label}: {result.stderr.strip()[:200]}")
        return False
    else:
        print(f"OK {label}")
        return True

# Check chunks
chunk_size = 50
for start in range(0, len(lines), chunk_size):
    end = min(start + chunk_size, len(lines))
    chunk = '\n'.join(lines[start:end])
    # Wrap in function to avoid global context issues
    test_code = f'(function(){{\n{chunk}\n}})()'
    if not check_syntax(test_code, f"Lines {start+1}-{end}"):
        # Found the bad chunk, narrow down
        for i in range(start, end):
            test_line = f'(function(){{\n{lines[i]}\n}})()'
            if not check_syntax(test_line, f"Line {i+1}"):
                # Show the exact line
                print(f"\nERROR ON LINE {i+1}:")
                print(lines[i][:300])
                sys.exit(0)
