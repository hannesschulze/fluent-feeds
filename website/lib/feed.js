import fs from 'fs';
import path from 'path';
import convert from 'xml-js';

// Based on: https://sreetamdas.com/blog/rss-for-nextjs

const feedsDirectory = path.join(process.cwd(), 'public/blog/feeds');
const blogTitle = 'Fluent Feeds Blog';
const blogDescription = 'A blog about the Fluent Feeds app.';
const blogCopyright = '© 2022, the Fluent Feeds developers'
const blogUrl = 'https://hannesschulze.github.io/fluent-feeds/blog';
const blogFeedName = 'atom.xml';
const blogFeedUrl = `${blogUrl}/feeds/${blogFeedName}`;

export async function generateFeeds(posts) {
  const atom = convert.js2xml({
    _declaration: {
      _attributes: {
        version: '1.0',
        encoding: 'utf-8'
      }
    },
    feed: {
      _attributes: {
        xmlns: 'http://www.w3.org/2005/Atom'
      },
      title: {
        _text: blogTitle
      },
      subtitle: {
        _text: blogDescription
      },
      link: [
        {
          _attributes: {
            rel: 'alternate',
            type: 'text/html',
            href: blogUrl
          }
        },
        {
          _attributes: {
            rel: 'self',
            type: 'application/atom+xml',
            href: blogFeedUrl
          }
        }
      ],
      id: {
        _text: blogUrl
      },
      updated: {
        _text: new Date().toISOString()
      },
      rights: {
        _text: blogCopyright
      },
      entry: posts.map(post => {
        const postUrl = `${blogUrl}/posts/${post.slug}`;
        const postTimestamp = new Date(post.timestamp);
        return {
          title: {
            _cdata: post.title
          },
          link: {
            _attributes: {
              rel: 'alternate',
              type: 'text/html',
              href: postUrl
            }
          },
          id: {
            _text: postUrl
          },
          published: {
            _text: postTimestamp.toISOString()
          },
          updated: {
            _text: postTimestamp.toISOString()
          },
          author: {
            name: {
              _text: post.author
            }
          },
          summary: {
            _attributes: {
              type: 'text'
            },
            _cdata: post.summary
          },
          content: {
            _attributes: {
              type: 'html',
              'xml:base': postUrl,
              'xml:lang': 'en'
            },
            _cdata: post.content
          }
        };
      })
    }
  }, {
    compact: true,
    ignoreComment: true,
    spaces: 2
  });

  await fs.promises.mkdir(feedsDirectory, { recursive: true });
  await fs.promises.writeFile(path.join(feedsDirectory, blogFeedName), atom);
}
