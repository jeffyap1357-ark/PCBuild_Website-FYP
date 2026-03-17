/**
 * Chatbot: keyword-based help, introduces all webpage features
 */
(function () {
    var wrap = document.getElementById('chatbot-wrap');
    var panel = document.getElementById('chatbot-panel');
    var messages = document.getElementById('chatbot-messages');
    var input = document.getElementById('chatbot-input');
    var sendBtn = document.getElementById('chatbot-send');
    var toggleBtn = document.getElementById('chatbot-toggle');
    var closeBtn = document.getElementById('chatbot-close');

    if (!wrap || !messages || !input) return;

    var isLoggedIn = document.body.classList.contains('chatbot-logged-in') ||
        (document.querySelector('a[href="/Build/Configure"]') !== null);

    var helpContent = {
        full: '<p><strong>PC Build Features:</strong></p>' +
            '<ul>' +
            '<li><a href="/">Home</a> — Quick access, top-rated hardware, quiz links</li>' +
            '<li><a href="/Hardware/List">Hardware</a> — Browse CPU, motherboard, GPU, RAM, storage; view details and prices</li>' +
            '<li><a href="/Learning/Index">Learning</a> — PC building tutorials and guides</li>' +
            '<li><a href="/Community/Index">Community</a> — View shared builds from others</li>' +
            '<li><a href="/Recommend">Recommend</a> — Get recommended builds by budget</li>' +
            (isLoggedIn ? '<li><a href="/Build/Configure">Build</a> — Pick compatible parts, save configs</li>' : '') +
            (isLoggedIn ? '<li><a href="/Build/Index">My Builds</a> — Manage saved builds</li>' : '') +
            (isLoggedIn ? '<li><a href="/Quiz/LevelsView">Quiz</a> — Beginner / Normal / Professional levels</li>' : '') +
            (isLoggedIn ? '<li><a href="/Performance/Index">Performance</a> — Select parts and get a performance score</li>' : '') +
            (!isLoggedIn ? '<li><a href="/Account/Login">Login</a> | <a href="/Account/Register">Register</a> — Log in to use Build, Quiz, Performance</li>' : '') +
            '</ul>',
        hardware: '<p><strong>Hardware</strong></p>' +
            '<p>Browse <a href="/Hardware/List">CPU, motherboard, GPU, RAM, storage</a> and more. View details, prices, specs. Compare items from the detail page.</p>',
        quiz: '<p><strong>Quiz</strong></p>' +
            '<p>Test your PC knowledge at <a href="/Quiz/LevelsView">Quiz</a>. Beginner, Normal, and Professional levels. One question per page with explanations. Login required.</p>',
        performance: '<p><strong>Performance</strong></p>' +
            '<p>At <a href="/Performance/Index">Performance Test</a> select CPU, GPU, etc. The system calculates a score and gives an assessment. Login required.</p>',
        build: '<p><strong>Build</strong></p>' +
            '<p>At <a href="/Build/Configure">Build</a> pick compatible CPU, motherboard, RAM, etc. Compatibility is checked. Save and share to community. Login required.</p>',
        learning: '<p><strong>Learning</strong></p>' +
            '<p>Read <a href="/Learning/Index">PC building tutorials</a> and step-by-step guides.</p>',
        community: '<p><strong>Community</strong></p>' +
            '<p>Browse <a href="/Community/Index">shared builds</a>, rate and comment. Share your build from <a href="/Build/Index">My Builds</a>.</p>',
        recommend: '<p><strong>Recommend</strong></p>' +
            '<p>At <a href="/Recommend">Recommend</a> enter your budget to get suggested hardware. Save or configure further.</p>',
        login: '<p><strong>Login / Register</strong></p>' +
            '<p><a href="/Account/Login">Login</a> | <a href="/Account/Register">Register</a> | <a href="/Account/ResetPassword">Reset Password</a></p>' +
            '<p>Log in to create builds, take quizzes, run performance tests, share to community.</p>',
        default: '<p>Type <strong>help</strong> for a full feature list. Or ask about: hardware, quiz, performance, build, learning, community, recommend, login.</p>'
    };

    var keywordMap = [
        { keys: ['help', 'what', 'features', 'intro'], reply: 'full' },
        { keys: ['hardware', 'cpu', 'gpu', 'motherboard', 'ram', 'storage', 'compare'], reply: 'hardware' },
        { keys: ['quiz', 'test', 'knowledge'], reply: 'quiz' },
        { keys: ['performance', 'score'], reply: 'performance' },
        { keys: ['build', 'pc build', 'configure'], reply: 'build' },
        { keys: ['learning', 'guide', 'tutorial'], reply: 'learning' },
        { keys: ['community', 'share'], reply: 'community' },
        { keys: ['recommend', 'budget'], reply: 'recommend' },
        { keys: ['login', 'register', 'account', 'sign'], reply: 'login' }
    ];

    function addMsg(text, isUser) {
        var div = document.createElement('div');
        div.className = 'chatbot-msg chatbot-msg-' + (isUser ? 'user' : 'bot');
        if (isUser) {
            div.textContent = text;
        } else {
            div.innerHTML = text;
        }
        messages.appendChild(div);
        messages.scrollTop = messages.scrollHeight;
    }

    function getReply(text) {
        var t = (text || '').trim().toLowerCase();
        if (!t) return helpContent.default;
        for (var i = 0; i < keywordMap.length; i++) {
            var item = keywordMap[i];
            for (var j = 0; j < item.keys.length; j++) {
                if (t.indexOf(item.keys[j].toLowerCase()) !== -1) {
                    return helpContent[item.reply] || helpContent.default;
                }
            }
        }
        return helpContent.default;
    }

    function send() {
        var text = input.value.trim();
        if (!text) return;
        addMsg(text, true);
        input.value = '';
        addMsg(getReply(text), false);
    }

    function toggle() {
        wrap.classList.toggle('chatbot-open');
        wrap.setAttribute('aria-hidden', wrap.classList.contains('chatbot-open') ? 'false' : 'true');
        if (wrap.classList.contains('chatbot-open')) {
            input.focus();
        }
    }

    toggleBtn && toggleBtn.addEventListener('click', toggle);
    closeBtn && closeBtn.addEventListener('click', toggle);
    sendBtn && sendBtn.addEventListener('click', send);
    input && input.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); send(); }
    });
})();
