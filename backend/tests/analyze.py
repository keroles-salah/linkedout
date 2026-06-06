import os, json

base = r'C:\Users\ke_ro\.openclaw-autoclaw\workspace\linkdout\backend\Linkdout.Api'

# Count files and lines
total_files = 0
total_lines = 0
by_type = {}
for root, dirs, files in os.walk(base):
    for f in files:
        if f.endswith('.cs') or f.endswith('.cshtml') or f.endswith('.json') or f.endswith('.csproj'):
            path = os.path.join(root, f)
            ext = f.split('.')[-1]
            try:
                with open(path, 'r', encoding='utf-8', errors='ignore') as fh:
                    content = fh.read()
                    lines = content.count('\n') + 1
                    total_lines += lines
                    total_files += 1
                    by_type[ext] = by_type.get(ext, 0) + 1
            except:
                pass

print(f'TOTAL: {total_files} files, {total_lines:,} lines')
print(f'By type: {json.dumps(by_type, indent=2)}')

# List controllers
print('\n=== CONTROLLERS ===')
controllers_dir = os.path.join(base, 'Controllers')
for f in sorted(os.listdir(controllers_dir)):
    if f.endswith('.cs'):
        path = os.path.join(controllers_dir, f)
        with open(path, 'r', encoding='utf-8', errors='ignore') as fh:
            content = fh.read()
            lines = content.count('\n') + 1
            is_api = 'ApiController' in content
            routes = []
            for line in content.split('\n'):
                if '[HttpGet' in line or '[HttpPost' in line:
                    route = line.strip()
                    routes.append(route[:80])
            print(f'  {f:<35} {lines:>4} lines  {"API" if is_api else "MVC":>4}  {len(routes)} routes')

# List views
print('\n=== VIEWS ===')
views_dir = os.path.join(base, 'Views')
for root, dirs, files in os.walk(views_dir):
    for f in sorted(files):
        if f.endswith('.cshtml'):
            path = os.path.join(root, f)
            rel = os.path.relpath(path, views_dir)
            with open(path, 'r', encoding='utf-8', errors='ignore') as fh:
                content = fh.read()
                lines = content.count('\n') + 1
                has_forms = '<form' in content
                has_js = '<script' in content
                print(f'  {rel:<40} {lines:>4} lines  Forms:{has_forms}  JS:{has_js}')

# List models
print('\n=== MODELS ===')
models_dir = os.path.join(base, 'Models')
for f in sorted(os.listdir(models_dir)):
    if f.endswith('.cs'):
        path = os.path.join(models_dir, f)
        with open(path, 'r', encoding='utf-8', errors='ignore') as fh:
            content = fh.read()
            lines = content.count('\n') + 1
            props = content.count('public ') - content.count('public class') - content.count('public interface')
            print(f'  {f:<35} {lines:>4} lines  {props} properties')
